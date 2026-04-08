using Godot;

public interface IComponentBase
{
    ComponentHolder Holder { get; }
    RigidBody2D Root { get; }

    /// <summary>
    /// Called after the root node runs its _Ready method.
    /// </summary>
    void AfterReady();
}