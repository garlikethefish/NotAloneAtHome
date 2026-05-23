using Godot;

namespace NotAloneAtHome.Components;

public partial class InteractableComponent : ComponentNode2D, IInteractable
{
    [Signal] public delegate void InteractedByEventHandler(GodotObject interactor);

    private Sprite2D _interactionSprite;
    private Vector2  _interactionKeyStartPos;
    private Vector2  _interactionKeyStartScale;
    private Tween    _appearTween;
    public Node Node => this;

    public override void _Ready()
    {
        base._Ready();
        _interactionSprite        = GetNode<Sprite2D>("InteractionKey");
        _interactionKeyStartPos   = _interactionSprite.Position;
        _interactionKeyStartScale = _interactionSprite.Scale;
        _interactionSprite.Visible = false;
        SpriteHideAnimation();
    }

    public void ShowSprite() => SpriteAppearAnimation();
    public void HideSprite() => SpriteHideAnimation();
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

    private void SpriteAppearAnimation()
    {
        _interactionSprite.Visible = true;
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.One, 0.5f);
    }

    private async void SpriteHideAnimation()
    {
        _appearTween?.Kill();
        _appearTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        _appearTween.TweenProperty(_interactionSprite, "scale", Vector2.Zero, 0.2f);
        await ToSignal(_appearTween, Tween.SignalName.Finished);
        _interactionSprite.Visible = false;
    }

    public void WhenInteractBy(IInteractor interactor)
    {
        if (!CanBeInteractedBy(interactor)) return;
        TweenAnimation();
        EmitSignal(SignalName.InteractedBy, interactor as GodotObject);
    }

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        return (Root as IInteractable)?.CanBeInteractedBy(interactor) ?? false;
    }
}