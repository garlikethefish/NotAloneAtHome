using Godot;

public interface IComponentInterface
{
    ComponentHolder Holder { get; }
    Node Node { get; }
}