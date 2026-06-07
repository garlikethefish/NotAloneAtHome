// NodeWireWeaver.cs
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class NodeWireWeaver : WeaverBase
{
    public override void Execute()
    {
        foreach (var type in ModuleDefinition.Types.Where(IsSceneClass))
        {
            var nodeFields = type.Fields
                .Where(f => f.CustomAttributes.Any(a => a.AttributeType.Name is "NodeAttribute" or "Node"))
                .ToList();

            if (nodeFields.Count == 0)
                continue;

            EnsureWireNodesMethod(type, nodeFields);
            EnsureWireNodesCalledInReady(type);
        }
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
            string nodePath = GetNodePath(field);

            var getNodeOrNull = new GenericInstanceMethod(getNodeOrNullOpen);
            getNodeOrNull.GenericArguments.Add(field.FieldType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, nodePath);
            il.Emit(OpCodes.Newobj, nodePathCtor);
            il.Emit(OpCodes.Callvirt, getNodeOrNull);
            il.Emit(OpCodes.Stfld, field);

            bool isMustAssign = field.CustomAttributes
                .Any(a => a.AttributeType.Name is "MustAssignAttribute" or "MustAssign");

            if (!isMustAssign)
            {
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
}