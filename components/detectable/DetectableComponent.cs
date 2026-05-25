using Godot;
using System.Collections.Generic;

public class DetectableComponentModel
{
    public DetectableComponent Detectable;
    public bool IsInLineOfSight;
}

public partial class DetectableComponent : ComponentStaticBody2D, IDetectable
{
    [Signal] public delegate void DetectedEventHandler(GodotObject detector);
    [Signal] public delegate void LostEventHandler(GodotObject detector);
    [Signal] public delegate void BecamePriorityEventHandler(GodotObject detector);
    [Signal] public delegate void LostPriorityEventHandler(GodotObject detector);
    [Export] public CollisionShape2D _collisionShape;
    public CollisionShape2D CollisionShape => _collisionShape;
    public Node Node => this;

    // public BlockerQueue CanBeDetectedBlockers { get; } = new();
    public List<AreaDetectorBase> Detectors = [];
    public List<AreaDetectorBase> SimpDetectors = []; // Detectors that have this detectable as priority

    public void EnterArea(AreaDetectorBase detector)
    {
        if (Detectors.Contains(detector)) return;
        Detectors.Add(detector);
        EmitSignal(SignalName.Detected, detector);
    }

    public void ExitArea(AreaDetectorBase detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        EmitSignal(SignalName.Lost, detector);
    }

    public void SetAsAreaPriority(AreaDetectorBase detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        EmitSignal(SignalName.BecamePriority, detector);
    }

    public void RemoveAsAreaPriority(AreaDetectorBase detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        EmitSignal(SignalName.LostPriority, detector);
    }

    public bool CanBeDetected(AreaDetectorBase detector)
    {
        return (Root as IDetectable).CanBeDetected(detector);
    }
}