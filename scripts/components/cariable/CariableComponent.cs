using Godot;

public partial class Carriable : ComponentNode2D, ICarriable
{
    [Signal] public delegate void PickedUpEventHandler(Node2D carrier);
    [Signal] public delegate void DroppedEventHandler();
    [Export] Sprite2D sprite;
    [Export] CollisionShape2D _collisionShape2D;
    [Export] RigidBody2D rigidBody;
    public CollisionShape2D CollisionShape2D => _collisionShape2D;
    public Node Node => this;

    // @onready var _detectable: IDetectable = get_fellow_helper(IDetectable)

    public async void PickUpBy(ICarrier carrier)
    {
        Root.SetDeferred("freeze", true);
        Root.Reparent(carrier.CarryPointNode, true);
        
        await PlayPickUpAnimation(Vector2.Zero);
        
        EmitSignal(SignalName.PickedUp, carrier.Node); // or C# event: OnPickUp?.Invoke(carrier);
        // _detectable?.CanBeDetectedBlockers.AddBlocker(this);
    }

    public async void DropAt(Vector2 landPos)
    {
        Root.Reparent(GetTree().CurrentScene, true);
        await PlayDropAnimation(landPos);

        Root.Freeze = false;
        EmitSignal(SignalName.Dropped);
    }
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

    public bool CanBeCarried(ICarrier carrier)
    {
        return (Root as ICarriable)?.CanBeCarried(carrier) ?? false;
    }

    // func retire_unc():
    // var parent = main_parent as RigidBody2D
    // parent.reparent(get_tree().current_scene, true)
    // if parent is RigidBody2D:
    // 	parent.freeze = false
    // if _detectable: _detectable.can_be_detected_blockers.remove_blocker(self)
    // carrier = null
}