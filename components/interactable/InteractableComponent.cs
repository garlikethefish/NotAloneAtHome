namespace NotAloneAtHome.Components;
using System;
using Godot;

#nullable enable
[Scene]
public partial class InteractableComponent : Node2D, IInteractableComponent
{
    [Export] public AnimationPlayer AnimationPlayer = default!; 
    public event Action<InteractorComponent>? OnInteraction;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.Play("disapear");
        if (this.ParentHas<DetectableComponent>(out var detectable))
        {
            detectable.OnBecameDetectorPriority += _ => AnimationPlayer.Play("apear");
            detectable.OnRemovedDetectorPriority += _ => AnimationPlayer.Play("disapear");
        }
    }

    public void HandleInteraction(InteractorComponent interactor)
    {
        AnimationPlayer.Play("interact");
        OnInteraction?.Invoke(interactor);
    }
}
