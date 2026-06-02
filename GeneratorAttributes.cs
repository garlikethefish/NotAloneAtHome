using System;

#nullable enable
[AttributeUsage(AttributeTargets.Class)]
public class SceneAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NodeAttribute : Attribute
{
    public string? Path { get; }
    public NodeAttribute(string? path = null) { Path = path; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MustAssignAttribute : Attribute
{

}
