using Godot;
using NotAloneAtHome.Components.Detectable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

public class DetectableComponentModel
{
    public IDetectable Detectable;
    public bool IsInLineOfSight;
}

#nullable enable
public partial class DetectableComponent : ComponentStaticBody2D, IDetectableComponent
{
    [Export] public CollisionShape2D? CollisionShape2D { get; private set; }
    public ReactiveList<IAreaDetector> BlacklistedDetectors { get; } = new();
    public List<IAreaDetector> Detectors = [];
    public List<IAreaDetector> SimpDetectors = []; // Detectors that have this detectable as priority
    private IDetectable RootDetectable => (IDetectable)Root;

    public override void _Ready()
    {
        base._Ready();
        BlacklistedDetectors.OnAdded += HandleAddToBlacklist;
        BlacklistedDetectors.OnRemoved += HandleRemoveFromBlacklist;
    }

    public override void _ExitTree()
    {
        HandleExitAllDetectors();
    }

    public void HandleEnterDetectorArea(IAreaDetector detector)
    {
        if (Detectors.Contains(detector) || BlacklistedDetectors.Contains(detector)) return;
        Detectors.Add(detector);
        RootDetectable.OnEnteredDetectorArea?.InvokeOrLog(detector);
    }

    public void HandleExitDetectorArea(IAreaDetector detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        RootDetectable.OnExitedDetectorArea?.InvokeOrLog(detector);
    }

    public void HandleSetAsDetectorPriority(IAreaDetector detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        RootDetectable.OnBecameDetectorPriority?.InvokeOrLog(detector);
    }

    public void HandleRemovedFromDetectorPriority(IAreaDetector detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        RootDetectable.OnRemovedDetectorPriority?.InvokeOrLog(detector);
    }

    public void HandleAddToBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.BlacklistDetectable(RootDetectable);
        detector.ExcludedRids.Add(RootDetectable.Rid);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void HandleRemoveFromBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Remove(detector);
        detector.ExcludedRids.Remove(RootDetectable.Rid);
        if (detector is Area2D area2D)
        {
            var bodies = area2D.GetOverlappingBodies();
            if (bodies.Contains(this)) detector.OnBodyEntered?.InvokeOrLog(this);
        }
    }

    public void HandleExitAllDetectors()
    {
        Detectors.ForEach(item => item.ForceUndetectDetectable(RootDetectable));
        SimpDetectors.ForEach(item => item.ForceUndetectDetectable(RootDetectable));
    }

    public Rid HandleGetRid()=>GetRid();
}