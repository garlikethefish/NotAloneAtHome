using Godot;

public interface IComponentBase
{
    ComponentHolder Holder { get; }
    Node2D Root { get; }
    Node Node { get; }
    /// <summary>
    /// Called after the root node runs its _Ready method.
    /// </summary>
    void AfterReady();
}