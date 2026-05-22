using Godot;
using System.Linq;

#nullable enable
public partial class CastedAreaDetectorComponent : AreaDetector, ICastedAreaDetector
{
    [Signal] public delegate void EnteredSightEventHandler(GodotObject detectable);
    [Signal] public delegate void ExitedSightEventHandler(GodotObject detectable);
    [Export] public int[] RaycastCollisionMasks = [10, 2];
    private DetectableComponent? _closestDetectable = null;
    public Color IsInSightColor  = Colors.Azure;
    public Color NotInSightColor = Colors.Crimson;
    public Color ClosestColor    = Colors.LimeGreen;

    public IDetectable? ClosestDetectable => _closestDetectable;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShowDebug) QueueRedraw();
        ValidateDetectables();

        var newPriority = GetClosestDetectable();

        if (newPriority != _closestDetectable)
        {
            _closestDetectable?.RemoveAsAreaPriority(this);
            newPriority?.SetAsAreaPriority(this);
        }

        _closestDetectable = newPriority;
    }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (_collisionShape.Shape is CircleShape2D circle)
            DrawCircle(_collisionShape.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (var model in DetectablesInArea)
        {
            var iDetectable  = model.Detectable;
            var localTarget = ToLocal(iDetectable.CollisionShape.GlobalPosition);

            Color color;
            if (iDetectable == _closestDetectable)
                color = ClosestColor;
            else if (model.IsInLineOfSight)
                color = IsInSightColor;
            else
                color = NotInSightColor;

            DrawCircle(localTarget, 2f, color);
            DrawLine(Vector2.Zero, localTarget, color, 0.5f);
        }
    }

    private void ValidateDetectables()
    {
        var excludedRids = ExcludedColliders
            .Select(c => c.GetRid())
            .ToList();

        var spaceState   = GetWorld2D().DirectSpaceState;
        var combinedMask = Utils.GetCombinedMask(RaycastCollisionMasks);

        for (int i = 0; i < DetectablesInArea.Count; i++)
        {
            var model = DetectablesInArea[i];

            var otherRids = DetectablesInArea
                .Where(m => m.Detectable != model.Detectable)
                .Select(m => m.Detectable.GetRid())
                .ToList();

            var allExcluded = new RidArray(excludedRids.Concat(otherRids));

            var query = PhysicsRayQueryParameters2D.Create(
                GlobalPosition,
                model.Detectable.CollisionShape.GlobalPosition,
                combinedMask,
                allExcluded
            );
            query.HitFromInside = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node2D>();
                if (collider is DetectableComponent detectable &&
                    detectable == model.Detectable &&
                    detectable.CanBeDetected(this))
                {
                    if (!model.IsInLineOfSight) EmitSignal(SignalName.EnteredSight, model.Detectable);
                    model.IsInLineOfSight = true;
                    continue;
                }
            }
    
            // wasnt hit or failed on hitting
            if (model.IsInLineOfSight) EmitSignal(SignalName.ExitedSight, model.Detectable);
            model.IsInLineOfSight = false;
        }
    }

    private DetectableComponent? GetClosestDetectable()
    {
        if (DetectablesInArea.Count == 0) return null;

        DetectableComponent? closest = null;

        foreach (var model in DetectablesInArea)
        {
            if (!model.IsInLineOfSight) continue;
            if (closest == null) { closest = model.Detectable; continue; }

            closest = (DetectableComponent)GetClosestNode(this, closest, model.Detectable);
        }

        return closest;
    }

    private Node2D GetClosestNode(Node2D point, Node2D first, Node2D second) =>
        point.GlobalPosition.DistanceTo(first.GlobalPosition) <=
        point.GlobalPosition.DistanceTo(second.GlobalPosition)
            ? first
            : second;
}