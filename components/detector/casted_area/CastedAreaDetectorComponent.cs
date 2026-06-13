namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
public partial class CastedAreaDetectorComponent : AreaDetectorComponent, ICastedAreaDetectorComponent
{
    [Export] public int[] RaycastCollisionMasks = [];
    public Color IsInSightColor  = Colors.Azure;
    public Color NotInSightColor = Colors.Crimson;
    public Color ClosestColor    = Colors.LimeGreen;
    public event Action<DetectableComponent>? OnSightEnter;
    public event Action<DetectableComponent>? OnSightExit;
    public DetectableComponent? ClosestDetectable { get; private set; }

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
            ClosestDetectable?.HandleRemovedFromDetectorPriority(this);
            newPriority?.HandleSetAsDetectorPriority(this);
        }

        ClosestDetectable = newPriority;
    }

    public override void WhenBodyExited(Node2D body)
    {
        if (!body.ParentHas<DetectableComponent>(out var detectable)) return;

        if (detectable == ClosestDetectable)
        {
            ClosestDetectable?.HandleRemovedFromDetectorPriority(this);
            ClosestDetectable = null;
            OnSightExit?.Invoke(detectable);
        }
        base.WhenBodyExited(body);
    }

    public override void HandleUndetectDetectable(DetectableComponent detectable)
    {
        if (detectable == ClosestDetectable)
        {
            ClosestDetectable?.HandleRemovedFromDetectorPriority(this);
            ClosestDetectable = null;
            OnSightExit?.Invoke(detectable);
        }
        base.HandleUndetectDetectable(detectable);
    }

    // public override string[] _GetConfigurationWarnings()
    // {
    //     GD.Print("warnings called, has child: " + this.HasChild<CollisionShape2D>());
    //     if (!this.HasChild<CollisionShape2D>())
    //         return ["A CollisionShape2D child is required."];

    //     return [];
    // }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (CollisionShape2D?.Shape is CircleShape2D circle)
            DrawCircle(CollisionShape2D.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (DetectableComponentModel model in DetectablesInArea.ToList())
        {
            DetectableComponent detectable  = model.Detectable;
            var localTarget = ToLocal(detectable.GlobalPosition);

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
                .Select(m => m.Detectable.HandleGetRid())
                .ToList();

            var allExcluded = new RidArray(excludedRids.Concat(otherRids));

            var query = PhysicsRayQueryParameters2D.Create(
                GlobalPosition,
                model.Detectable.GlobalPosition,
                combinedMask,
                allExcluded
            );
            query.HitFromInside = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node2D>();
                if (collider.ParentHas<DetectableComponent>(out var detectable) &&
                    detectable == model.Detectable &&
                    detectable.CanDetect(this)
                ) {
                    if (!model.IsInLineOfSight) OnSightEnter?.Invoke(model.Detectable);
                    model.IsInLineOfSight = true;
                    continue;
                }
            }
    
            // wasnt hit or failed on hitting
            if (model.IsInLineOfSight) OnSightExit?.Invoke(model.Detectable);
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