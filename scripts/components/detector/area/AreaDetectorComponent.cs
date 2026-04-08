using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AreaDetector : ComponentArea2D, IAreaDetector
{
    [Signal] public delegate void EnteredEventHandler(GodotObject detectable);
    [Signal] public delegate void ExitedEventHandler(GodotObject detectable);
    [Export] public CollisionObject2D[] ExcludedColliders = [];
    [Export] public bool ShowDebug = true;
    [Export] public int[] CollisionMasks = [10, 2];
    public List<DetectableComponentModel> DetectablesInArea = [];
    protected CollisionShape2D _collisionShape;
    public Node Node => this;

    public override void _Ready()
    {
        base._Ready();
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ShowDebug) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!ShowDebug) return;

        if (_collisionShape.Shape is CircleShape2D circle)
            DrawCircle(_collisionShape.Position, circle.Radius, new Color(1, 0, 0, 0.12f));

        foreach (var model in DetectablesInArea)
        {
            var detectable = model.Detectable;
            var localTarget = ToLocal(detectable.CollisionShape.GlobalPosition);

            Color color = new(0, 1, 0, 0.5f);
            DrawCircle(localTarget, 2f, color);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not DetectableComponent detectable) return;
        if (DetectablesInArea.Any(detInArea => detInArea.Detectable == detectable)) return;

        DetectablesInArea.Add(new DetectableComponentModel { Detectable = detectable });
        detectable.EnterArea(this);
        EmitSignal(SignalName.Entered, body);
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is not DetectableComponent detectable) return;

        var model = DetectablesInArea.FirstOrDefault(m => m.Detectable == detectable);
        if (model == null) return;

        DetectablesInArea.Remove(model);
        detectable.ExitArea(this);
        EmitSignal(SignalName.Exited, body);
    }

    /// <summary>
    /// Determines if the detector can detect the given detectable.
    /// Is checked on each detectable
    /// </summary>
    public bool CanDetectLike(DetectableComponent detectable)
    {
        return (Root as IAreaDetector)?.CanDetectLike(detectable) ?? false;
    }
}