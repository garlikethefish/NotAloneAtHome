using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class ModuleWeaver : BaseModuleWeaver
{
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System";
        yield return "netstandard";
    }

    public override void Execute()
    {
        foreach (var type in ModuleDefinition.Types.Where(IsSceneClass))
        {
            var nodeFields = type.Fields
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType.Name is "NodeAttribute" or "Node"))
                .ToList();

            if (nodeFields.Count == 0)
                continue;

            var mustAssignFields = nodeFields
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType.Name is "MustAssignAttribute" or "MustAssign"))
                .ToList();

            EnsureWireNodesMethod(type, nodeFields);
            EnsureWireNodesCalledInReady(type);

            if (mustAssignFields.Count > 0)
            {
                EnsureConfigurationWarnings(type, mustAssignFields);
                EnsureValidateProperty(type);
                EnsureEditorReady(type);
            }
        }
    }

    private bool IsSceneClass(TypeDefinition type)
    {
        return type.HasCustomAttributes &&
               type.CustomAttributes.Any(a => a.AttributeType.Name.Contains("Scene"));
    }

    private void EnsureWireNodesMethod(TypeDefinition type, List<FieldDefinition> nodeFields)
    {
        var existing = type.Methods.FirstOrDefault(m => m.Name == "WireNodes");
        if (existing != null)
            type.Methods.Remove(existing);

        var method = new MethodDefinition(
            "WireNodes",
            MethodAttributes.Public | MethodAttributes.HideBySig,
            ModuleDefinition.TypeSystem.Void);

        var il = method.Body.GetILProcessor();

        var getNodeOrNullOpen = FindGetNodeOrNull();
        var nodePathCtor = FindNodePathCtor();
        var exceptionCtor = ModuleDefinition.ImportReference(
            typeof(System.Exception).GetConstructor(new[] { typeof(string) }));

        foreach (var field in nodeFields)
        {
            var attr = field.CustomAttributes
                .First(a => a.AttributeType.Name is "NodeAttribute" or "Node");

            string nodePath = attr.ConstructorArguments.Count > 0
                ? attr.ConstructorArguments[0].Value as string ?? field.Name
                : field.Name;

            var getNodeOrNull = new GenericInstanceMethod(getNodeOrNullOpen);
            getNodeOrNull.GenericArguments.Add(field.FieldType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, nodePath);
            il.Emit(OpCodes.Newobj, nodePathCtor);
            il.Emit(OpCodes.Callvirt, getNodeOrNull);
            il.Emit(OpCodes.Stfld, field);

            bool isMustAssign = field.CustomAttributes.Any(a => a.AttributeType.Name is "MustAssignAttribute" or "MustAssign");

            if (!isMustAssign)
            {
                // required — throw if missing
                var afterThrow = il.Create(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Brtrue_S, afterThrow);
                il.Emit(OpCodes.Ldstr, $"WireNodes: missing node for {field.Name}");
                il.Emit(OpCodes.Newobj, exceptionCtor);
                il.Emit(OpCodes.Throw);
                il.Append(afterThrow);
            }
        }

        il.Emit(OpCodes.Ret);
        type.Methods.Add(method);
    }

    private void EnsureWireNodesCalledInReady(TypeDefinition type)
    {
        var readyMethod = type.Methods.FirstOrDefault(m => m.Name == "_Ready");
        var wireNodes = type.Methods.First(m => m.Name == "WireNodes");

        if (readyMethod == null)
        {
            readyMethod = new MethodDefinition(
                "_Ready",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                ModuleDefinition.TypeSystem.Void);

            var il = readyMethod.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, wireNodes);
            il.Emit(OpCodes.Ret);
            type.Methods.Add(readyMethod);
        }
        else
        {
            var il = readyMethod.Body.GetILProcessor();

            var baseReadyCall = readyMethod.Body.Instructions
                .FirstOrDefault(i => i.OpCode == OpCodes.Call &&
                    i.Operand is MethodReference mr && mr.Name == "_Ready");

            if (baseReadyCall != null)
            {
                var ldarg = il.Create(OpCodes.Ldarg_0);
                var call = il.Create(OpCodes.Call, wireNodes);
                il.InsertAfter(baseReadyCall, call);
                il.InsertAfter(baseReadyCall, ldarg);
            }
            else
            {
                var first = readyMethod.Body.Instructions.First();
                var ldarg = il.Create(OpCodes.Ldarg_0);
                var call = il.Create(OpCodes.Call, wireNodes);
                il.InsertBefore(first, ldarg);
                il.InsertBefore(first, call);
            }
        }
    }

    private void EnsureConfigurationWarnings(TypeDefinition type, List<FieldDefinition> mustAssignFields)
    {
        // string[] return type
        var stringType = ModuleDefinition.TypeSystem.String;
        var stringArrayType = new ArrayType(stringType);

        var getNodeOrNullOpen = FindGetNodeOrNull();
        var nodePathCtor = FindNodePathCtor();

        var existingMethod = type.Methods.FirstOrDefault(m => m.Name == "_GetConfigurationWarnings");

        if (existingMethod == null)
        {
            // Generate fresh method
            var method = new MethodDefinition(
                "_GetConfigurationWarnings",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                stringArrayType);

            var il = method.Body.GetILProcessor();

            // List<string> warnings = new List<string>()
            var listType = ModuleDefinition.ImportReference(typeof(List<string>));
            var listCtor = ModuleDefinition.ImportReference(typeof(List<string>).GetConstructor(System.Type.EmptyTypes));
            var listAdd = ModuleDefinition.ImportReference(typeof(List<string>).GetMethod("Add"));
            var listToArray = ModuleDefinition.ImportReference(typeof(List<string>).GetMethod("ToArray"));

            var warningsVar = new VariableDefinition(listType);
            method.Body.Variables.Add(warningsVar);
            method.Body.InitLocals = true;

            il.Emit(OpCodes.Newobj, listCtor);
            il.Emit(OpCodes.Stloc, warningsVar);

            foreach (var field in mustAssignFields)
            {
                var attr = field.CustomAttributes
                    .First(a => a.AttributeType.Name is "NodeAttribute" or "Node");

                string nodePath = attr.ConstructorArguments.Count > 0
                    ? attr.ConstructorArguments[0].Value as string ?? field.Name
                    : field.Name;

                var getNodeOrNull = new GenericInstanceMethod(getNodeOrNullOpen);
                getNodeOrNull.GenericArguments.Add(field.FieldType);

                // if (GetNodeOrNull<T>("path") == null) warnings.Add("...")
                var afterAdd = il.Create(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, nodePath);
                il.Emit(OpCodes.Newobj, nodePathCtor);
                il.Emit(OpCodes.Callvirt, getNodeOrNull);
                il.Emit(OpCodes.Brtrue_S, afterAdd);
                il.Emit(OpCodes.Ldloc, warningsVar);
                il.Emit(OpCodes.Ldstr, $"{field.Name} must be assigned (child node '{nodePath}' not found)");
                il.Emit(OpCodes.Callvirt, listAdd);
                il.Append(afterAdd);
            }

            // return warnings.ToArray()
            il.Emit(OpCodes.Ldloc, warningsVar);
            il.Emit(OpCodes.Callvirt, listToArray);
            il.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }
        else
        {
            // Merge — prepend checks before existing return
            var il = existingMethod.Body.GetILProcessor();

            var listType = ModuleDefinition.ImportReference(typeof(List<string>));
            var listCtor = ModuleDefinition.ImportReference(typeof(List<string>).GetConstructor(System.Type.EmptyTypes));
            var listAdd = ModuleDefinition.ImportReference(typeof(List<string>).GetMethod("Add"));

            // Find the first Ret instruction to inject before it
            var firstRet = existingMethod.Body.Instructions.First(i => i.OpCode == OpCodes.Ret);

            foreach (var field in mustAssignFields)
            {
                var attr = field.CustomAttributes
                    .First(a => a.AttributeType.Name is "NodeAttribute" or "Node");

                string nodePath = attr.ConstructorArguments.Count > 0
                    ? attr.ConstructorArguments[0].Value as string ?? field.Name
                    : field.Name;

                var getNodeOrNull = new GenericInstanceMethod(getNodeOrNullOpen);
                getNodeOrNull.GenericArguments.Add(field.FieldType);

                var afterAdd = il.Create(OpCodes.Nop);
                il.InsertBefore(firstRet, il.Create(OpCodes.Ldarg_0));
                il.InsertBefore(firstRet, il.Create(OpCodes.Ldstr, nodePath));
                il.InsertBefore(firstRet, il.Create(OpCodes.Newobj, nodePathCtor));
                il.InsertBefore(firstRet, il.Create(OpCodes.Callvirt, getNodeOrNull));
                il.InsertBefore(firstRet, il.Create(OpCodes.Brtrue_S, afterAdd));
                il.InsertBefore(firstRet, il.Create(OpCodes.Ldstr, $"{field.Name} must be assigned (child node '{nodePath}' not found)"));
                il.InsertBefore(firstRet, afterAdd);
            }
        }
    }

    private void EnsureValidateProperty(TypeDefinition type)
    {
        if (type.Methods.Any(m => m.Name == "_ValidateProperty"))
            return;

        var dictionaryType = FindGodotDictionary();
        var updateWarnings = FindUpdateConfigurationWarnings(type);

        var method = new MethodDefinition(
            "_ValidateProperty",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            ModuleDefinition.TypeSystem.Void);

        method.Parameters.Add(new ParameterDefinition("property", ParameterAttributes.None, dictionaryType));

        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, updateWarnings);
        il.Emit(OpCodes.Ret);

        type.Methods.Add(method);
    }

    private void EnsureEditorReady(TypeDefinition type)
    {
        // Inject Engine.IsEditorHint() + UpdateConfigurationWarnings() into _Ready
        var readyMethod = type.Methods.FirstOrDefault(m => m.Name == "_Ready");
        if (readyMethod == null) return;

        var isEditorHint = FindIsEditorHint();
        var updateWarnings = FindUpdateConfigurationWarnings(type);

        var il = readyMethod.Body.GetILProcessor();
        var last = readyMethod.Body.Instructions.Last(); // Ret

        var afterUpdate = il.Create(OpCodes.Nop);

        // if (Engine.IsEditorHint()) UpdateConfigurationWarnings()
        il.InsertBefore(last, il.Create(OpCodes.Call, isEditorHint));
        il.InsertBefore(last, il.Create(OpCodes.Brfalse_S, afterUpdate));
        il.InsertBefore(last, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(last, il.Create(OpCodes.Callvirt, updateWarnings));
        il.InsertBefore(last, afterUpdate);
    }

    private MethodReference FindGetNodeOrNull()
    {
        foreach (var asm in ModuleDefinition.AssemblyReferences)
        {
            var resolved = ModuleDefinition.AssemblyResolver.Resolve(asm);
            if (resolved == null) continue;
            foreach (var type in resolved.MainModule.Types)
            {
                if (type.FullName != "Godot.Node") continue;
                var method = type.Methods.FirstOrDefault(m =>
                    m.Name == "GetNodeOrNull" && m.HasGenericParameters);
                if (method != null)
                    return ModuleDefinition.ImportReference(method);
            }
        }
        throw new WeavingException("Could not find Godot.Node.GetNodeOrNull<T>");
    }

    private MethodReference FindNodePathCtor()
    {
        foreach (var asm in ModuleDefinition.AssemblyReferences)
        {
            var resolved = ModuleDefinition.AssemblyResolver.Resolve(asm);
            if (resolved == null) continue;
            foreach (var type in resolved.MainModule.Types)
            {
                if (type.FullName != "Godot.NodePath") continue;
                var ctor = type.Methods.FirstOrDefault(m =>
                    m.IsConstructor &&
                    m.Parameters.Count == 1 &&
                    m.Parameters[0].ParameterType.FullName == "System.String");
                if (ctor != null)
                    return ModuleDefinition.ImportReference(ctor);
            }
        }
        throw new WeavingException("Could not find Godot.NodePath(string) constructor");
    }

    private TypeReference FindGodotDictionary()
    {
        foreach (var asm in ModuleDefinition.AssemblyReferences)
        {
            var resolved = ModuleDefinition.AssemblyResolver.Resolve(asm);
            if (resolved == null) continue;
            foreach (var type in resolved.MainModule.Types)
            {
                if (type.FullName == "Godot.Collections.Dictionary")
                    return ModuleDefinition.ImportReference(type);
            }
        }
        throw new WeavingException("Could not find Godot.Collections.Dictionary");
    }

    private MethodReference FindUpdateConfigurationWarnings(TypeDefinition type)
    {
        foreach (var asm in ModuleDefinition.AssemblyReferences)
        {
            var resolved = ModuleDefinition.AssemblyResolver.Resolve(asm);
            if (resolved == null) continue;
            foreach (var t in resolved.MainModule.Types)
            {
                if (t.FullName != "Godot.Node") continue;
                var method = t.Methods.FirstOrDefault(m => m.Name == "UpdateConfigurationWarnings");
                if (method != null)
                    return ModuleDefinition.ImportReference(method);
            }
        }
        throw new WeavingException("Could not find Godot.Node.UpdateConfigurationWarnings");
    }

    private MethodReference FindIsEditorHint()
    {
        foreach (var asm in ModuleDefinition.AssemblyReferences)
        {
            var resolved = ModuleDefinition.AssemblyResolver.Resolve(asm);
            if (resolved == null) continue;
            foreach (var type in resolved.MainModule.Types)
            {
                if (type.FullName != "Godot.Engine") continue;
                var method = type.Methods.FirstOrDefault(m => m.Name == "IsEditorHint");
                if (method != null)
                    return ModuleDefinition.ImportReference(method);
            }
        }
        throw new WeavingException("Could not find Godot.Engine.IsEditorHint");
    }

    public override bool ShouldCleanReference => true;
}