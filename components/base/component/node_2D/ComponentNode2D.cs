using System.Linq;
using Godot;
using NotAloneAtHome.Components.Base.Holder;

public partial class ComponentNode2D : Node2D, IComponentBase
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

