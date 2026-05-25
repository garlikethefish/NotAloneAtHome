using Godot;

public interface IComponentInterface
{
    ComponentHolder Holder { get; }
    Node Node { get; } // return node bc pisshit godot cant emit interfaces through signals
    Node2D Root { get; }
}