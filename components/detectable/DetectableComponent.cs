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
    private IDetectable RootDetectable => (IDetectable)Root;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _ExitTree()
    {
        HandleExitAllDetectors();
    }

    public void HandleEnterDetectorArea(IAreaDetector detector)
    {
        if (Detectors.Contains(detector) || BlacklistedDetectors.Contains(detector)) return;
        Detectors.Add(detector);
        RootDetectable.WhenEnteredDetectorArea(detector);
    }

    public void HandleExitDetectorArea(IAreaDetector detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        RootDetectable.WhenExitedDetectorArea(detector);
    }

    public void HandleSetAsDetectorPriority(IAreaDetector detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        RootDetectable.WhenSetAsDetectorPriority(detector);
    }

    public void HandleRemovedFromDetectorPriority(IAreaDetector detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        RootDetectable.WhenRemovedFromDetectorPriority(detector);
    }

    public void HandleAddToBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.BlacklistDetectable(RootDetectable);
        detector.ExcludeRid(RootDetectable.Rid);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void HandleRemoveFromBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Remove(detector);
        detector.IncludeRid(RootDetectable.Rid);
        if (detector is Area2D area2D)
        {
            var bodies = area2D.GetOverlappingBodies();
            if (bodies.Contains(this)) detector.WhenBodyEntered(this);
        }
    }

    public bool HandleIsDetectorBlacklisted(IAreaDetector detector)
    {
        return BlacklistedDetectors.Contains(detector);
    }

    public void HandleExitAllDetectors()
    {
        Detectors.ForEach(item => item.ExitDetectable(RootDetectable));
        SimpDetectors.ForEach(item => item.ExitDetectable(RootDetectable));
    }

    public Rid HandleGetRid()=>GetRid();
}