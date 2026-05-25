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

    public List<IAreaDetector> BlacklistedDetectors { get; } = [];

    public List<IAreaDetector> Detectors = [];
    public List<IAreaDetector> SimpDetectors = []; // Detectors that have this detectable as priority

    public void WhenEnteredDetectorArea(IAreaDetector detector)
    {
        if (Detectors.Contains(detector) || BlacklistedDetectors.Contains(detector)) return;
        Detectors.Add(detector);
        EmitSignal(SignalName.Detected, detector.Node);
    }

    public void WhenExitedDetectorArea(IAreaDetector detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        EmitSignal(SignalName.Lost, detector.Node);
    }

    public void WhenSetAsDetectorPriority(IAreaDetector detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        EmitSignal(SignalName.BecamePriority, detector.Node);
    }

    public void WhenRemovedFromDetectorPriority(IAreaDetector detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        EmitSignal(SignalName.LostPriority, detector.Node);
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return (Root as IDetectable).CanBeDetected(detector);
    }

    public void AddToBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.WhenBlacklistedFromDetectable(this);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void RemoveFromBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Remove(detector);
        if (detector.Node is Area2D area2D)
        {
            var bodies = area2D.GetOverlappingBodies();
            if (bodies.Contains(this)) detector.OnBodyEntered(this);
        }
    }

    public bool IsDetectorBlacklisted(IAreaDetector detector)
    {
        return BlacklistedDetectors.Contains(detector);
    }
}