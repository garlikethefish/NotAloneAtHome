namespace NotAloneAtHome.Components;
using System;
using Godot;

#nullable enable
[Scene]
public partial class InteractableComponent : Node2D, IInteractableComponent
{
    [Export] public AnimationPlayer AnimationPlayer = default!; 
    public event Action<InteractorComponent>? OnInteractionFrom;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.Play("disappear");
        if (this.ParentHas<DetectableComponent>(out var detectable))
        {
            detectable.OnBecameDetectorPriority += _ => Appear();
            detectable.OnRemovedDetectorPriority += _ => Disappear();
        }
    }

    void Appear() {
        AnimationPlayer.SpeedScale = 3;
        AnimationPlayer.Play("appear");
    }

    void Disappear() {
        AnimationPlayer.SpeedScale = 3;
        AnimationPlayer.Play("disappear");
    }

    public void HandleInteraction(InteractorComponent interactor)
    {
        AnimationPlayer.SpeedScale = 3;
        AnimationPlayer.Seek(0);
        AnimationPlayer.Play("interact");
        OnInteractionFrom?.Invoke(interactor);
    }
}
