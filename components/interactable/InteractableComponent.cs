namespace NotAloneAtHome.Components;
using System;
using Godot;
#nullable enable

[Scene]
public partial class InteractableComponent : Node2D, IInteractableComponent
{
    [Export] public AnimationPlayer AnimationPlayer = default!;

    // FIX: Make the offset editable from the Godot Inspector
    [Export] private Vector2 _screenOffset = new(0, -24);

    [Node("ScreenAnchor")]
    private CanvasLayer _screenAnchor = default!;

    [Node("ScreenAnchor/KeyAnchor")]
    private Node2D _keyAnchor = default!;

    public event Action<InteractorComponent>? OnInteractionFrom;

    public override void _Ready()
    {
        base._Ready();
        _screenAnchor.Layer = 3;
        AnimationPlayer.Play("disappear");

        if (this.ParentHas<DetectableComponent>(out var detectable))
        {
            detectable.OnBecameDetectorPriority += _ => Appear();
            detectable.OnRemovedDetectorPriority += _ => Disappear();
        }
    }

    public override void _Process(double delta)
    {
        var camera = GetViewport().GetCamera2D();
        if (camera == null)
            return;

        Vector2 worldPos = GlobalPosition + _screenOffset;
        Vector2 screenPos = GetViewport().GetCanvasTransform() * worldPos;
        _keyAnchor.Position = screenPos;
        _keyAnchor.Scale = camera.Zoom;
    }

    void Appear()
    {
        AnimationPlayer.SpeedScale = 3;
        AnimationPlayer.Play("appear");
    }

    void Disappear()
    {
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