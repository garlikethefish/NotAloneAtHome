using Godot;
using NotAloneAtHome.Components.Detectable;
using System;
using System.Collections.Generic;

public class DetectableComponentModel
{
    public IDetectable Detectable;
    public bool IsInLineOfSight;
}

#nullable enable
public partial class DetectableComponent : ComponentStaticBody2D, IDetectableComponent
{
    [Export] public CollisionShape2D? CollisionShape2D { get; private set; }
    public List<IAreaDetector> BlacklistedDetectors { get; } = [];
    public List<IAreaDetector> Detectors = [];
    public List<IAreaDetector> SimpDetectors = []; // Detectors that have this detectable as priority
    public IDetectable RootDetectable => (IDetectable)Root;

    public Rid Rid => GetRid();

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _ExitTree()
    {
        ExitAllDetectors();
    }

    public void WhenEnteredDetectorArea(IAreaDetector detector)
    {
        if (Detectors.Contains(detector) || BlacklistedDetectors.Contains(detector)) return;
        Detectors.Add(detector);
        RootDetectable.OnDetectableEnteredDetectorArea(detector);
    }

    public void WhenExitedDetectorArea(IAreaDetector detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        RootDetectable.OnDetectableExitedDetectorArea(detector);
    }

    public void WhenSetAsDetectorPriority(IAreaDetector detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        RootDetectable.OnDetectableSetAsDetectorPriority(detector);
    }

    public void WhenRemovedFromDetectorPriority(IAreaDetector detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        RootDetectable.OnDetectableRemovedFromDetectorPriority(detector);
    }

    public void AddToBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.WhenBlacklistedFromDetectable(RootDetectable);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void RemoveFromBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Remove(detector);
        if (detector is Area2D area2D)
        {
            var bodies = area2D.GetOverlappingBodies();
            if (bodies.Contains(this)) detector.OnBodyEntered(this);
        }
    }

    public bool IsDetectorBlacklisted(IAreaDetector detector)
    {
        return BlacklistedDetectors.Contains(detector);
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return RootDetectable?.CanBeDetected(detector) ?? false;
    }

    public void ExitAllDetectors()
    {
        Detectors.ForEach(item => item.RemoveDetectable(RootDetectable));
        SimpDetectors.ForEach(item => item.RemoveDetectable(RootDetectable));
    }
}