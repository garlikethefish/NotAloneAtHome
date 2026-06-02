namespace NotAloneAtHome.Components;
using System;
using Godot;

#nullable enable
public partial class InteractableAnimationComponent : Node2D, IInteractableAnimationComponent
{
    [Export] private Sprite2D _interactionSprite = default!;
    private Vector2 _interactionKeyStartPos;
    private Vector2 _interactionKeyStartScale;
    private Tween? _appearTween;
    public override void _Ready()
    {
        base._Ready();
        _interactionKeyStartPos   = _interactionSprite.Position;
        _interactionKeyStartScale = _interactionSprite.Scale;
        _interactionSprite.Visible = false;
        HideInteractSprite();

        if (this.ParentHas<DetectableComponent>(out var detectable))
        {
            detectable.OnBecameDetectorPriority += _ => ShowInteractSprite();
            detectable.OnRemovedDetectorPriority += _ => HideInteractSprite();
        }

        if (this.ParentHas<InteractableComponent>(out var interactable))
        {
            interactable.OnInteraction += _ => PerformInteraction();
        }
    }

    public void ShowInteractSprite()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion()) return;
        GD.Print("showed!");
        _interactionSprite.Visible = true;
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.One, 0.5f);
    }

    public void HideInteractSprite()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion()) return;
        GD.Print("hidden!");
        
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.Zero, 0.2f);
        ToSignal(_appearTween, Tween.SignalName.Finished);
        _appearTween.TweenCallback(Callable.From(() => _interactionSprite.Visible = false));
    }

    public void PerformInteraction()
    {
        _interactionSprite.Position = _interactionKeyStartPos;
        _interactionSprite.Scale    = _interactionKeyStartScale;

        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_interactionSprite, "position", _interactionKeyStartPos - new Vector2(0, -20), 0.1f);
        tween.TweenProperty(_interactionSprite, "scale",    _interactionKeyStartScale / 2,                 0.1f);
        tween.TweenProperty(_interactionSprite, "position", _interactionKeyStartPos,                       0.1f).SetDelay(0.1f);
        tween.TweenProperty(_interactionSprite, "scale",    _interactionKeyStartScale,                     0.1f).SetDelay(0.1f);
    }
}