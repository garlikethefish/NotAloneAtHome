// MustAssignWeaver.cs
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class MustAssignWeaver : WeaverBase
{
    public override void Execute()
    {
        foreach (var type in ModuleDefinition.Types.Where(IsSceneClass))
        {
            var mustAssignFields = type.Fields
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType.Name is "NodeAttribute" or "Node") &&
                            f.CustomAttributes.Any(a => a.AttributeType.Name is "MustAssignAttribute" or "MustAssign"))
                .ToList();

            if (mustAssignFields.Count == 0)
                continue;

            EnsureConfigurationWarnings(type, mustAssignFields);
            EnsureValidateProperty(type);
            EnsureEditorReady(type);
        }
    }

    private void EnsureConfigurationWarnings(TypeDefinition type, List<FieldDefinition> mustAssignFields)
    {
        var stringType = ModuleDefinition.TypeSystem.String;
        var stringArrayType = new ArrayType(stringType);

        var getNodeOrNullOpen = FindGetNodeOrNull();
        var nodePathCtor = FindNodePathCtor();

        var existingMethod = type.Methods.FirstOrDefault(m => m.Name == "_GetConfigurationWarnings");

        if (existingMethod == null)
        {
            var method = new MethodDefinition(
                "_GetConfigurationWarnings",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                stringArrayType);

            var il = method.Body.GetILProcessor();

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
                string nodePath = GetNodePath(field);
                var getNodeOrNull = new GenericInstanceMethod(getNodeOrNullOpen);
                getNodeOrNull.GenericArguments.Add(field.FieldType);

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

            il.Emit(OpCodes.Ldloc, warningsVar);
            il.Emit(OpCodes.Callvirt, listToArray);
            il.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }
        else
        {
            var il = existingMethod.Body.GetILProcessor();
            var listAdd = ModuleDefinition.ImportReference(typeof(List<string>).GetMethod("Add"));
            var firstRet = existingMethod.Body.Instructions.First(i => i.OpCode == OpCodes.Ret);

            foreach (var field in mustAssignFields)
            {
                string nodePath = GetNodePath(field);
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
        var updateWarnings = FindUpdateConfigurationWarnings();

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
        var readyMethod = type.Methods.FirstOrDefault(m => m.Name == "_Ready");
        if (readyMethod == null) return;

        var isEditorHint = FindIsEditorHint();
        var updateWarnings = FindUpdateConfigurationWarnings();

        var il = readyMethod.Body.GetILProcessor();
        var last = readyMethod.Body.Instructions.Last();

        var afterUpdate = il.Create(OpCodes.Nop);
        il.InsertBefore(last, il.Create(OpCodes.Call, isEditorHint));
        il.InsertBefore(last, il.Create(OpCodes.Brfalse_S, afterUpdate));
        il.InsertBefore(last, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(last, il.Create(OpCodes.Callvirt, updateWarnings));
        il.InsertBefore(last, afterUpdate);
    }
}