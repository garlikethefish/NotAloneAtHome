namespace NotAloneAtHome.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
#nullable enable
[Scene]
public partial class AreaDetectorComponent : Area2D, IAreaDetectorComponent
{
    [Node] public CollisionShape2D CollisionShape2D { get; private set; } = default!;
    [Export] public bool ShowDebug = false;    
    public List<Rid> ExcludedRids { get; set; } = [];
    public List<DetectableComponentModel> DetectablesInArea { get; } = [];
    public event Action<DetectableComponent>? OnBodyEntered;
    public event Action<DetectableComponent>? OnBodyExited;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += WhenBodyEntered;
        BodyExited += WhenBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShowDebug) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (CollisionShape2D?.Shape is CircleShape2D circle)
            DrawCircle(CollisionShape2D.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (var model in DetectablesInArea.ToList())
        {
            var detectable = model.Detectable;
            var localTarget = ToLocal(detectable.GlobalPosition);

            Color color = new(0, 1, 0, 0.5f);
            DrawCircle(localTarget, 2f, color);
        }
    }

    public virtual void WhenBodyEntered(Node2D body)
    {
        if (!body.ParentHas<DetectableComponent>(out var detectable) 
            || !detectable.IsDetectable
            || detectable.BlacklistedDetectors.Contains(this)
            || DetectablesInArea.Any(detInArea => detInArea.Detectable == detectable)
        ) return;

        DetectablesInArea.Add(new DetectableComponentModel { Detectable = detectable });
        detectable.HandleEnterDetectorArea(this);
        OnBodyEntered?.Invoke(detectable);
    }

    public virtual void WhenBodyExited(Node2D body)
    {
        if (!body.ParentHas<DetectableComponent>(out var detectable)) return;

        var model = DetectablesInArea.FirstOrDefault(m => m.Detectable == detectable);
        if (model == null) return;

        DetectablesInArea.Remove(model);
        detectable.HandleExitDetectorArea(this);
        OnBodyExited?.Invoke(detectable);
    }

    public virtual void HandleUndetectDetectable(DetectableComponent detectable)
    {
        WhenBodyExited(detectable);
    }

    public void HandleTryDetectDetectable(DetectableComponent detectable)
    {
        var bodies = GetOverlappingBodies();
        if (bodies.Contains(detectable)) WhenBodyEntered(detectable);
    }

    public void HandleAttemptToEnterArea(DetectableComponent detectable)
    {
        var bodies = GetOverlappingBodies();
        if (bodies.Contains(detectable)) WhenBodyEntered(detectable); 
    }
}