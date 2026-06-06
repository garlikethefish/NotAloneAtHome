// WeaverBase.cs
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

public abstract class WeaverBase : BaseModuleWeaver
{
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System";
        yield return "netstandard";
    }

    public override bool ShouldCleanReference => true;

    protected bool IsSceneClass(TypeDefinition type) =>
        type.HasCustomAttributes &&
        type.CustomAttributes.Any(a => a.AttributeType.Name.Contains("Scene"));

    protected MethodReference FindGetNodeOrNull()
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

    protected MethodReference FindNodePathCtor()
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

    protected MethodReference FindUpdateConfigurationWarnings()
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

    protected MethodReference FindIsEditorHint()
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

    protected TypeReference FindGodotDictionary()
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

    protected static string GetNodePath(FieldDefinition field)
    {
        var attr = field.CustomAttributes
            .First(a => a.AttributeType.Name is "NodeAttribute" or "Node");
        return attr.ConstructorArguments.Count > 0
            ? attr.ConstructorArguments[0].Value as string ?? field.Name
            : field.Name;
    }
}