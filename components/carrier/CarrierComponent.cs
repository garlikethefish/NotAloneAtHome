using Godot;
using System.Linq;

public partial class CarrierComponent : ComponentNode2D, ICarrierComponent
{
    [Export] public CollisionObject2D[] ExcludedColliders = [];
    [Export] public float Reach = 40f;
    [Export] public float StepSize = 5f;
    [Export] public float StartOffset = 12f;
    [Export] public int RayCount = 16;
    [Export] public bool ShowDebug = true;
    [Export] public int[] CollisionMasks = [];
    [Export] public Node2D CarryPointNode { get; private set; }
    public bool IsAnimating => IsPickingUp || IsDropping;
    public bool IsCarrying  => Carriable != null;
    public ICarriable Carriable { get; private set; }
    public bool IsPickingUp = false;
    public bool IsDropping = false;
    public Vector2 FacingDirection = Vector2.Inf;
    Vector2 _validCarriableDropPosition = Vector2.Inf;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        if (ShowDebug) QueueRedraw();
        if (Carriable == null) return;

        _validCarriableDropPosition = Casters.RadialyFindAValidLocationWhichFitsGivenShape(
            GetWorld2D().DirectSpaceState,
            Carriable.CollisionShape2D,
            GlobalPosition,
            Reach,
            StepSize,
            StartOffset,
            RayCount,
            Utils.GetCombinedMask(CollisionMasks),
            new RidArray(ExcludedColliders.Select(c => c.GetRid()).ToArray())
        );
    }

    public void HandlePickup(ICarriable carriable)
    {
        Carriable = carriable;
        IsPickingUp = true;
        carriable.OnPickedUpBy?.InvokeOrLog((ICarrier)Root);

        if (carriable is IDetectable detectable)
            detectable.BlacklistedDetectors.Add((IAreaDetector)Root);
    }

    public void HandleDrop()
    {
        var carriable = Carriable;
        Carriable = null;
        IsDropping = true;
        carriable.OnDropedAt?.InvokeOrLog(_validCarriableDropPosition);

        if (carriable is IDetectable detectable)
            detectable.BlacklistedDetectors.Remove((IAreaDetector)Root);
    }
}