namespace NotAloneAtHome.Components;
using System;
using Godot;

#nullable enable
public partial class InteractableComponent : ComponentNode2D, IInteractableComponent
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
        HideSprite();

        GD.Print("Is root detectable?: ", Root is IDetectable );
        if (Root is IDetectable detectable)
        {
            detectable.OnDetectableBecamePriority += _ => ShowSprite();
            detectable.OnDetectableLostPriority += _ => HideSprite();
        }
    }

    private void TweenAnimation()
    {
        _interactionSprite.Position = _interactionKeyStartPos;
        _interactionSprite.Scale    = _interactionKeyStartScale;

        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_interactionSprite, "position", _interactionKeyStartPos - new Vector2(0, -20), 0.1f);
        tween.TweenProperty(_interactionSprite, "scale",    _interactionKeyStartScale / 2,                 0.1f);
        tween.TweenProperty(_interactionSprite, "position", _interactionKeyStartPos,                       0.1f).SetDelay(0.1f);
        tween.TweenProperty(_interactionSprite, "scale",    _interactionKeyStartScale,                     0.1f).SetDelay(0.1f);
    }

    private void ShowSprite()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion()) return;
        GD.Print("showed!");
        _interactionSprite.Visible = true;
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.One, 0.5f);
    }

    private async void HideSprite()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion()) return;
        GD.Print("hidden!");
        
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.Zero, 0.2f);
        await ToSignal(_appearTween, Tween.SignalName.Finished);
        _interactionSprite.Visible = false;
    }

    public void HandleInteractedBy(IInteractor interactor)
    {
        TweenAnimation();
    }
}