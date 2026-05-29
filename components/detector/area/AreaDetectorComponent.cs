using System.Collections.Generic;
using System.Linq;
using Godot;
using NotAloneAtHome.Components.Detectable;

public partial class AreaDetectorComponent : ComponentArea2D, IAreaDetectorComponent
{
    [Export] public CollisionShape2D CollisionShape { get; private set; }
    [Export] public bool ShowDebug = true;
    [Export] public int[] CollisionMasks = [10, 2];
    public List<Rid> ExcludedRids { get; } = [];
    public List<DetectableComponentModel> DetectablesInArea { get; } = [];
    private IAreaDetector RootDetector => (IAreaDetector)Root;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShowDebug) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (CollisionShape.Shape is CircleShape2D circle)
            DrawCircle(CollisionShape.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (var model in DetectablesInArea.ToList())
        {
            var detectable = model.Detectable;
            var localTarget = ToLocal(detectable.CollisionShape2D.GlobalPosition);

            Color color = new(0, 1, 0, 0.5f);
            DrawCircle(localTarget, 2f, color);
        }
    }

    public void OnBodyEntered(Node2D body)
    {
        GD.Print($"[OnBodyEntered] body={body?.Name ?? "NULL"}, valid={IsInstanceValid(body)}");
        if (!body.TryGetRoot<IDetectable>(out var detectable) 
            || detectable.IsDetectorBlacklisted(RootDetector)
        ) return;
        if (DetectablesInArea.Any(detInArea => detInArea.Detectable == detectable)) return;

        DetectablesInArea.Add(new DetectableComponentModel { Detectable = detectable });
        detectable.WhenEnteredDetectorArea(RootDetector);
        RootDetector.WhenBodyEntered(body);
    }

    public void OnBodyExited(Node2D body)
    {
        if (!body.TryGetRoot<IDetectable>(out var detectable)) return;

        var model = DetectablesInArea.FirstOrDefault(m => m.Detectable == detectable);
        if (model == null) return;

        DetectablesInArea.Remove(model);
        detectable.WhenExitedDetectorArea(RootDetector);
        RootDetector.WhenBodyExited(body);
    }

    public void HandleBlacklistDetectable(IDetectable detectable)
    {
        OnBodyExited((Node2D)detectable);
    }

    public virtual void HandleExitDetectable(IDetectable detectable)
    {
        DetectablesInArea.RemoveAll(model => model.Detectable == detectable);
    }

    public void HandleExcludeRid(Rid rid)
    {
        ExcludedRids.Add(rid);
    }

    public void HandleIncludeRid(Rid rid)
    {
        ExcludedRids.Remove(rid);
    }
}