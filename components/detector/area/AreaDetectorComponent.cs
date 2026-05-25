using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AreaDetectorComponent : ComponentArea2D, IAreaDetector
{
    [Signal] public delegate void EnteredEventHandler(GodotObject detectable);
    [Signal] public delegate void ExitedEventHandler(GodotObject detectable);
    [Export] public CollisionObject2D[] ExcludedColliders { get; private set; } = [];
    [Export] public bool ShowDebug = true;
    [Export] public int[] CollisionMasks = [10, 2];
    public List<DetectableComponentModel> DetectablesInArea { get; } = [];
    public CollisionShape2D CollisionShape { get; private set; }
    public Node Node => this;

    public override void _Ready()
    {
        base._Ready();
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
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

        foreach (var model in DetectablesInArea)
        {
            var detectable = model.Detectable;
            var localTarget = ToLocal(detectable.CollisionShape.GlobalPosition);

            Color color = new(0, 1, 0, 0.5f);
            DrawCircle(localTarget, 2f, color);
        }
    }

    public void OnBodyEntered(Node2D body)
    {
        if (body is not IDetectable detectable || detectable.IsDetectorBlacklisted(this)) return;
        if (DetectablesInArea.Any(detInArea => detInArea.Detectable == detectable)) return;

        DetectablesInArea.Add(new DetectableComponentModel { Detectable = detectable as DetectableComponent });
        detectable.WhenEnteredDetectorArea(this);
        EmitSignal(SignalName.Entered, body);
    }

    public void OnBodyExited(Node2D body)
    {
        if (body is not IDetectable detectable) return;

        var model = DetectablesInArea.FirstOrDefault(m => m.Detectable == detectable);
        if (model == null) return;

        DetectablesInArea.Remove(model);
        detectable.WhenExitedDetectorArea(this);
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

    public void WhenBlacklistedFromDetectable(IDetectable detectable)
    {
        OnBodyExited(detectable.Node as Node2D);
    }
}