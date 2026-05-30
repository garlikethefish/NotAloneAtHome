namespace NotAloneAtHome.Components;

using Godot;

public partial class ComponentArea2D : Area2D, IComponentBase
{
    public ComponentHolder Holder { get; private set; }
    public Node2D Root => Holder.Root;

    public Node2D Node2D => this;

    public override void _Ready()
    {
        Holder = GetParent<ComponentHolder>();
    }
    public virtual void AfterReady() { }
}