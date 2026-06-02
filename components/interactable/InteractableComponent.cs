namespace NotAloneAtHome.Components;
using System;
using Godot;

#nullable enable
public partial class InteractableComponent : Node2D, IInteractableComponent
{
    [Export] public Script AnimationScript = default!; 
    public event Action<InteractorComponent>? OnInteraction;

    public override void _Ready()
    {
    //    AnimationScript._Ready();
    }

    public void HandleInteraction(InteractorComponent interactor)
    {
        OnInteraction?.Invoke(interactor);
    }
}
