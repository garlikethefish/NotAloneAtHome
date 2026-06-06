namespace NotAloneAtHome.Components;

using System;
using Godot;
public partial class ThrowerComponent : Node2D, IThrowerComponent
{
    [Export] public bool ShowDebug = false;
    [Export] private Node2D _targetSpriteNode;
    public float MaxThrowRange = 100f;
    public bool IsCharging = false;
    public float CurrentCharge = 0f;
    public float NormalizedCharge => CurrentCharge / MaxChargeSeconds;
    public float MaxChargeSeconds = 1f;
    public float ChargeMultiplier = 1f;
    public int[] AimColliderMasks = [2];
    public Vector2 FacingDirection { get; set; }
    public bool IsAiming { get; private set; }
    Vector2 AimedAtLocation = Vector2.Inf;
    public event Action<ThrowableComponent> OnThrow;

    public override void _Ready()
    {
        base._Ready();
        _targetSpriteNode.Visible = false;
    }

    public override void _Process(double delta)
    {
        _targetSpriteNode.GlobalPosition = AimedAtLocation;

        if (!IsCharging) return;
        CurrentCharge = Mathf.Clamp(CurrentCharge + (float)delta * ChargeMultiplier,0, MaxChargeSeconds);
        AimedAtLocation = CalculateAimedAtLocation(FacingDirection);

        if (ShowDebug) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!ShowDebug || AimedAtLocation == Vector2.Inf) return;

        var localTarget = ToLocal(AimedAtLocation);
        DrawCircle(localTarget, 12f, new Color(0, 1, 0, 0.5f));
        DrawLine(Vector2.Zero, localTarget, Colors.White, 2f);
    }

    Vector2 CalculateAimedAtLocation(Vector2 direction)
    {
        var aimedAtLocation = GlobalPosition + direction.Normalized() * MaxThrowRange * NormalizedCharge;
        var adjustedLocation = GetCircleShotCenter(aimedAtLocation, 12f);
        return adjustedLocation;
    }

    private Vector2 GetCircleShotCenter(Vector2 targetGlobalPos, float radius)
    {
        var spaceState = GetWorld2D().DirectSpaceState;

        var shape        = new CircleShape2D { Radius = radius };
        var query        = new PhysicsShapeQueryParameters2D();
        query.Shape          = shape;
        query.Transform      = new Transform2D(0, GlobalPosition);
        query.Motion         = targetGlobalPos - GlobalPosition;
        query.CollisionMask  = Utils.GetCombinedMask(AimColliderMasks);

        var result         = spaceState.CastMotion(query);
        var travelFraction = result[0];
        return GlobalPosition + query.Motion * travelFraction;
    }

    public void HandleThrow(ThrowableComponent throwable)
    {
        throwable.GetParent().Reparent(GetTree().CurrentScene);

        if (throwable.ParentHas<DetectableComponent>(out var detectable)
            && this.ParentHas<AreaDetectorComponent>(out var detector)
        ) {
            detectable.HandleUnblacklistDetector(detector);
            detector.ExcludedRids.Remove(detectable.HandleGetRid());
        }
            
        throwable.HandleThrownBy(this, AimedAtLocation);
        OnThrow?.Invoke(throwable);
        HandleStopAiming();
    }

    public void HandleStartAiming()
    {
        CurrentCharge = 0;
        IsCharging    = true;
        _targetSpriteNode.Visible = true;
    }

    public void HandleStopAiming()
    {
        IsCharging      = false;
        CurrentCharge   = 0;
        AimedAtLocation = Vector2.Inf;
        
        _targetSpriteNode.GlobalPosition = Vector2.Inf;
        _targetSpriteNode.Visible = false;
        QueueRedraw();
    }
}