namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
public partial class CastedAreaDetectorComponent : AreaDetectorComponent, ICastedAreaDetectorComponent
{
    [Export] public int[] RaycastCollisionMasks = [10, 2];
    public Color IsInSightColor  = Colors.Azure;
    public Color NotInSightColor = Colors.Crimson;
    public Color ClosestColor    = Colors.LimeGreen;
    public IDetectable? ClosestDetectable { get; private set; }
    public ICastedAreaDetector RootCastedDetector => (ICastedAreaDetector)Root;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShowDebug) QueueRedraw();
        ValidateDetectables();

        var newPriority = GetClosestDetectable();

        if (newPriority != ClosestDetectable)
        {
            ClosestDetectable?.OnRemovedDetectorPriority?.InvokeOrLog(RootCastedDetector);
            newPriority?.OnBecameDetectorPriority?.InvokeOrLog(RootCastedDetector);
        }

        ClosestDetectable = newPriority;
    }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (CollisionShape.Shape is CircleShape2D circle)
            DrawCircle(CollisionShape.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (var model in DetectablesInArea.ToList())
        {
            var detectable  = model.Detectable;
            var localTarget = ToLocal(detectable.CollisionShape2D.GlobalPosition);

            Color color;
            if (detectable == ClosestDetectable)
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
        if (DetectablesInArea.Count == 0) return;

        var excludedRids = ExcludedRids.ToList();

        var spaceState   = GetWorld2D().DirectSpaceState;
        var combinedMask = Utils.GetCombinedMask(RaycastCollisionMasks);

        var DetectablesInAreaSnapshot = DetectablesInArea.ToList();

        for (int i = 0; i < DetectablesInAreaSnapshot.Count; i++)
        {
            var model = DetectablesInAreaSnapshot[i];

            var otherRids = DetectablesInAreaSnapshot
                .Where(m => m.Detectable != model.Detectable)
                .Select(m => m.Detectable.Rid)
                .ToList();

            var allExcluded = new RidArray(excludedRids.Concat(otherRids));

            var query = PhysicsRayQueryParameters2D.Create(
                GlobalPosition,
                model.Detectable.CollisionShape2D.GlobalPosition,
                combinedMask,
                allExcluded
            );
            query.HitFromInside = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node2D>();
                if (collider.TryGetRoot<IDetectable>(out var detectable) &&
                    detectable == model.Detectable &&
                    detectable.CanBeDetected(RootCastedDetector)
                ) {
                    if (!model.IsInLineOfSight) RootCastedDetector.OnEnteredSight?.InvokeOrLog(model.Detectable);
                    model.IsInLineOfSight = true;
                    continue;
                }
            }
    
            // wasnt hit or failed on hitting
            if (model.IsInLineOfSight) RootCastedDetector.OnExitedSight?.InvokeOrLog(model.Detectable);
            model.IsInLineOfSight = false;
        }
    }

    private IDetectable? GetClosestDetectable()
    {
        if (DetectablesInArea.Count == 0) return null;

        IDetectable? closest = null;

        foreach (var model in DetectablesInArea)
        {
            if (!model.IsInLineOfSight) continue;
            if (closest == null) { closest = model.Detectable; continue; }

            closest = (IDetectable)GetClosestNode(this, (Node2D)closest, (Node2D)model.Detectable);
        }

        return closest;
    }

    private Node2D GetClosestNode(Node2D point, Node2D first, Node2D second) =>
        point.GlobalPosition.DistanceTo(first.GlobalPosition) <=
        point.GlobalPosition.DistanceTo(second.GlobalPosition)
            ? first
            : second;

    public override void HandleForceUndetectDetectable(IDetectable detectable)
    {
        if (ClosestDetectable == detectable)
        ClosestDetectable = null;
    }

    public void HandleBlacklistDetectable(IDetectable detectable)
    {
        throw new NotImplementedException();
    }
}