namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Linq;

#nullable enable
[Tool]
public partial class CarrierComponent : Node2D, ICarrierComponent
{
    [Export] public CollisionObject2D[] ExcludedColliders = [];
    [Export] public float Reach = 40f;
    [Export] public float StepSize = 5f;
    [Export] public float StartOffset = 12f;
    [Export] public int RayCount = 16;
    [Export] public bool ShowDebug = true;
    [Export] public int[] CollisionMasks = [];
    [Export] public Node2D CarryPointNode { get; private set; } = default!;
    public event Action<CarriableComponent>? OnCarriableAssigned;
    public event Action<CarriableComponent>? OnCarriableRemoved;
    public event Action? OnDrop;
    public event Action<CarriableComponent>? OnPickup;
    public bool IsCarrying  => CarriableComp != null;
    private CarriableComponent? _carriableComponent;
    public CarriableComponent? CarriableComp {
        get => _carriableComponent;
        private set
        {
            if (value == null)
            {
                _unsubFromCarriable?.Invoke();
                _unsubFromCarriable = null;
                if (_carriableComponent != null) OnCarriableRemoved?.Invoke(_carriableComponent);
            }
            else
            {
                _unsubFromCarriable?.Invoke();
                _unsubFromCarriable = value.OnDestroyed(OnCarriableExitTree);
                OnCarriableAssigned?.Invoke(value);
            }
            _carriableComponent = value;
        }
    }
    private Action? _unsubFromCarriable;
    public Vector2 FacingDirection = Vector2.Inf;
    Vector2 _validCarriableDropPosition = Vector2.Inf;
   
    public override void _Ready()
    {
        base._Ready();

        if (this.ParentHas<ThrowerComponent>(out var thrower))
        {
            thrower.OnThrow += throwable =>
            {
                if (throwable.ParentHas<CarriableComponent>(out var car) && car == CarriableComp)
                {
                    CarriableComp = null;
                }  
            };
        }
    }

    public override void _Process(double delta)
    {
        if (ShowDebug) QueueRedraw();
        if (!IsInstanceValid(CarriableComp)) return;

        _validCarriableDropPosition = Casters.RadialyFindAValidLocationWhichFitsGivenShape(
            GetWorld2D().DirectSpaceState,
            CarriableComp.CollisionShape2D,
            GlobalPosition,
            Reach,
            StepSize,
            StartOffset,
            RayCount,
            Utils.GetCombinedMask(CollisionMasks),
            new RidArray(ExcludedColliders.Select(c => c.GetRid()).ToArray())
        );
    }

    public void HandlePickup(CarriableComponent carriableComponent)
    {
        CarriableComp = carriableComponent;
        CarriableComp.HandlePickedUpBy(this);
        OnPickup?.Invoke(CarriableComp);

        if (CarriableComp.ParentHas<DetectableComponent>(out var detectable)
            && this.ParentHas<AreaDetectorComponent>(out var detector)
        ) {
            detectable.IsDetectable = false;
            detectable.HandleBlacklistDetector(detector);
            detector.ExcludedRids.Add(detectable.HandleGetRid());
        }
    }

    public void HandleDrop()
    {
        if (!IsInstanceValid(CarriableComp)) return;
        CarriableComp.HandleDropedAt(_validCarriableDropPosition);
        OnDrop?.Invoke();

        if (CarriableComp.ParentHas<DetectableComponent>(out var detectable)
            && this.ParentHas<AreaDetectorComponent>(out var detector)
        ) {
            detectable.IsDetectable = true;
            detectable.HandleUnblacklistDetector(detector);
            detector.ExcludedRids.Remove(detectable.HandleGetRid());
        }

        CarriableComp = null;
    }

    void OnCarriableExitTree()
    {
        CarriableComp = null;
        OnDrop?.Invoke();
    }
}