namespace NotAloneAtHome.Components;

using System;
using Godot;

public partial class CariableComponent : ComponentNode2D, ICarriableComponent
{
    [Export] public CollisionShape2D CollisionShape2D { get; private set; }
    public event Action<ICarrier> OnPickedUpBy;
    public event Action<Vector2> OnDropedAt;

    private SignalAwaiter PlayDropAnimation(Vector2 landPos)
    {
        var tween  = CreateTween();
        var tweenX = CreateTween();
        
        tween.SetParallel().SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Root, "global_position:y", Root.GlobalPosition.Y - 20, 0.2f)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(Root, "scale", new Vector2(0.5f, 0.5f), 0.2f)
            .SetTrans(Tween.TransitionType.Quad);
        
        tweenX.TweenProperty(Root, "global_position:x", landPos.X, 0.4f);
        
        tween.Chain();
        tween.TweenProperty(Root, "global_position:y", landPos.Y, 0.2f)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(Root, "scale", Vector2.One, 0.2f)
            .SetTrans(Tween.TransitionType.Quad);
        
        return ToSignal(tween, Tween.SignalName.Finished);
    }

    private SignalAwaiter PlayPickUpAnimation(Vector2 endPos)
    {        
        var tween  = CreateTween();
        var tweenX = CreateTween();
        
        tween.SetParallel().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(Root, "scale", new Vector2(0.3f, 0.3f), 0.2f)
            .SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(Root, "position:y", Root.Position.Y - 20, 0.2f);
        
        tweenX.TweenProperty(Root, "position:x", endPos.X, 0.4f);
        
        tween.Chain();
        tween.TweenProperty(Root, "position:y", endPos.Y, 0.2f);
        tween.TweenProperty(Root, "scale", Vector2.One, 0.2f)
            .SetTrans(Tween.TransitionType.Expo);
        
        return ToSignal(tween, Tween.SignalName.Finished);
    }

    public async void HandlePickedUpBy(ICarrier carrier)
    {
        Root.Reparent(carrier.CarrierComponent.CarryPointNode, true);
        await PlayPickUpAnimation(carrier.CarrierComponent.CarryPointNode.Position);
        OnPickedUpBy?.Invoke(carrier);
    }

    public async void HandleDropedAt(Vector2 landPos)
    {
        Root.Reparent(GetTree().CurrentScene, true);
        await PlayDropAnimation(landPos);
        OnDropedAt?.Invoke(landPos);
    }
}