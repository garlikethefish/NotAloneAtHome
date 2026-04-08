using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Carrier : ComponentNode2D, ICarrier
{
    [Signal] public delegate void PickedUpEventHandler(GodotObject carriable);
    [Signal] public delegate void DroppedEventHandler(GodotObject carriable);
    [Export] public CollisionObject2D[] ExcludedColliders = [];
    [Export] public float Reach       = 40f;
    [Export] public float StepSize    = 5f;
    [Export] public float StartOffset = 12f;
    [Export] public int   RayCount    = 16;
    [Export] public bool  ShowDebug   = true;
    [Export] public int[] CollisionMasks = [];
    [Export] public Node2D CarryPointNode { get; private set; }

    public Node Node => this;
    public bool IsAnimating => IsPickingUp || IsDropping;
    public bool IsCarrying  => _carriable != null;
    public bool IsPickingUp = false;
    public bool IsDropping = false;
    public Vector2 FacingDirection = Vector2.Inf;
    Vector2 _validCarriableDropPosition = Vector2.Inf;
    private ICarriable _carriable;
    // private List<(Vector2 From, Vector2 To)> _debugRays = new();
    // private List<(Vector2 Pos, bool IsValid)> _debugPoints = new();

    public override void _Ready()
    {
        base._Ready();
        // _thrower       = Holder.Get<Thrower>();
    }

    public override void _Process(double delta)
    {
        if (ShowDebug) QueueRedraw();
        if (_carriable == null) return;

        _validCarriableDropPosition = Casters.RadialyFindAValidLocationWhichFitsGivenShape(
            GetWorld2D().DirectSpaceState,
            _carriable.CollisionShape2D,
            GlobalPosition,
            Reach,
            StepSize,
            StartOffset,
            RayCount,
            Utils.GetCombinedMask(CollisionMasks),
            new RidArray(ExcludedColliders.Select(c => c.GetRid()).ToArray())
        );
    }

    // public override void _Draw()
    // {
    //     foreach (var ray in _debugRays)
    //         DrawLine(ToLocal(ray.From), ToLocal(ray.To), new Color(0, 1, 1, 0.3f), 1f);

    //     foreach (var point in _debugPoints)
    //     {
    //         var color = point.IsValid ? Colors.Green : Colors.Red;
    //         DrawCircle(ToLocal(point.Pos), 3f, color);
    //     }

    //     if (_validCarriableDropPosition != Vector2.Inf)
    //     {
    //         var localPos = ToLocal(_validCarriableDropPosition);
    //         DrawCircle(localPos, 6f, Colors.Yellow);
    //         DrawArc(localPos, 10f, 0, Mathf.Tau, 32, Colors.Yellow, 2f);
    //     }
    // }

    // public void ToggleCarry(Carriable carriable)
    // {
    //     if (!IsCarrying) TryToCarry(carriable);
    //     else             TryToDrop();
    // }

    // public bool TryToDrop()
    // {
    //     if (_carriable == null ||
    //         _validCarriableDropPosition == Vector2.Inf ||
    //         IsAnimating) return false;

    //     Drop();
    //     return true;
    // }

    // private void Drop()
    // {
    //     IsDropping = true;
    //     _carriable.OnDrop += OnDropFinished;
    //     _carriable.Drop(_validCarriableDropPosition);
    //     EmitSignal(SignalName.OnCarryStop, _carriable);
    //     _carriable = null;
    // }

    // private void OnDropFinished(ICarrier carrier)
    // {
    //     IsDropping = false;
    //     _carriable.OnDrop -= OnDropFinished;
    // }

    // public bool TryToCarry(Carriable carriable)
    // {
    //     if (carriable == null   ||
    //         IsCarrying          ||
    //         !carriable.CanBeCarried(this) ||
    //         !CanCarry(carriable)||
    //         IsAnimating) return false;

    //     Carry(carriable);
    //     return true;
    // }

    // private void Carry(Carriable carriable)
    // {
    //     IsPickingUp = true;
    //     _thrower?.CanBeThrownBlockers.AddBlocker(this);

    //     carriable.OnPickUp += OnPickUpFinished;

    //     _carriable = carriable;
    //     _carriable.PickUp(this);
    //     EmitSignal(SignalName.OnCarryStart, carriable);
    // }

    // private void OnPickUpFinished(ICarrier carrier)
    // {
    //     IsPickingUp = false;
    //     _thrower?.CanBeThrownBlockers.RemoveBlocker(this);
    //     _carriable.OnPickUp -= OnPickUpFinished;
    // }

    // public void RetireUncarried()
    // {
    //     if (_carriable == null) return;
    //     _carriable.RetireUncarried();
    //     _carriable = null;
    // }

    public void Pickup(ICarriable carriable)
    {
        IsPickingUp = true;
        // _thrower?.CanBeThrownBlockers.AddBlocker(this);
        carriable.PickUpBy(this);

        // carriable.OnPickUp += OnPickUpFinished;

        EmitSignal(SignalName.PickedUp, carriable.Node);
        _carriable = carriable;
    }

    public void Drop()
    {
        var carriable = _carriable;
        _carriable = null;

        IsDropping = true;
        // _carriable.OnDrop += OnDropFinished;
        carriable.DropAt(_validCarriableDropPosition);
        EmitSignal(SignalName.Dropped, carriable.Node);
    }
}