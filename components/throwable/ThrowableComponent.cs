namespace NotAloneAtHome.Components;

using System;
using Godot;
    
public partial class ThrowableComponent : Node2D, IThrowableComponent
{
    [Signal] public delegate void ThrownEventHandler(GodotObject thrower);
    [Signal] public delegate void LandedEventHandler(Vector2 position);
    [Export] public Sprite2D Sprite;
    public bool IsFlying { get; private set; }
    private float _flyDuration = 0.6f;

    public event Action<ThrowerComponent, Vector2> OnThrownBy;
    public event Action<Vector2> OnLanded;

    public override void _Ready()
    {
        base._Ready();
    }

    private void StartFlyAnimation(Vector2 landPos)
    {
        if (landPos == Vector2.Inf) return;

        var tween = CreateTween();
        tween.TweenProperty(GetParent(), "global_position", landPos, _flyDuration)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        if (Sprite == null) return;

        var originalScale  = Sprite.Scale;
        var spriteTween    = CreateTween().SetParallel().SetEase(Tween.EaseType.InOut);
        var rotationTween  = CreateTween();

        rotationTween.TweenProperty(Sprite, "rotation", Mathf.DegToRad(360), _flyDuration);

        spriteTween.TweenProperty(Sprite, "position:y", Sprite.Position.Y - 25, _flyDuration / 2).AsRelative();
        spriteTween.TweenProperty(Sprite, "scale", new Vector2(0.7f, 0.7f), _flyDuration / 2);

        spriteTween.Chain();
        spriteTween.TweenProperty(Sprite, "position:y", Sprite.Position.Y + 25, _flyDuration / 2).AsRelative();
        spriteTween.TweenProperty(Sprite, "scale", originalScale, _flyDuration / 2);

        rotationTween.TweenCallback(Callable.From(() =>
        {
            Sprite.Rotation = 0;
            Land();
        }));
    }

    public void HandleThrownBy(ThrowerComponent thrower, Vector2 toPosition)
    {
        IsFlying = true;

        if (this.ParentHas<CarriableComponent>(out var carriable))
        {
            carriable.IsCarried = false;
        }

        StartFlyAnimation(toPosition);
        OnThrownBy?.Invoke(thrower, toPosition);
    }

    public void Land()
    {
        IsFlying = false;
        OnLanded?.Invoke(GlobalPosition);
    }
}