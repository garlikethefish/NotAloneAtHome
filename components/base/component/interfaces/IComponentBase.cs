namespace NotAloneAtHome.Components;

using Godot;


public interface IComponentBase
{
    ComponentHolder Holder { get; }
    Node2D Root { get; }
    Node2D Node2D { get; }
    /// <summary>
    /// Called after the root node runs its _Ready method.
    /// </summary>
    void AfterReady();
    protected virtual void WireComponents() { }
}