namespace NotAloneAtHome.Components;

using Godot;
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
    public ReactiveList<IAreaDetector> BlacklistedDetectors { get; } = new();
    public List<IAreaDetector> Detectors = [];
    public List<IAreaDetector> SimpDetectors = []; // Detectors that have this detectable as priority

    public event Action<IAreaDetector>? OnEnteredDetectorArea;
    public event Action<IAreaDetector>? OnExitedDetectorArea;
    public event Action<IAreaDetector>? OnBecameDetectorPriority;
    public event Action<IAreaDetector>? OnRemovedDetectorPriority;

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
        OnEnteredDetectorArea?.Invoke(detector);
    }

    public void HandleExitDetectorArea(IAreaDetector detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        OnExitedDetectorArea?.Invoke(detector);
    }

    public void HandleSetAsDetectorPriority(IAreaDetector detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        OnBecameDetectorPriority?.Invoke(detector);
    }

    public void HandleRemovedFromDetectorPriority(IAreaDetector detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        OnRemovedDetectorPriority?.Invoke(detector);
    }

    public void HandleAddToBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.AreaDetectorComponent.HandleBlacklistDetectable(RootDetectable);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void HandleRemoveFromBlacklist(IAreaDetector detector)
    {
        BlacklistedDetectors.Remove(detector);
        detector.AreaDetectorComponent.ExcludedRids.Remove(GetRid());
        if (detector is Area2D area2D)
        {
            var bodies = area2D.GetOverlappingBodies();
            if (bodies.Contains(this)) detector.OnBodyEntered?.InvokeOrLog(this);
        }
    }

    public void HandleExitAllDetectors()
    {
        Detectors.ForEach(item => item.AreaDetectorComponent.HandleForceUndetectDetectable(RootDetectable));
        SimpDetectors.ForEach(item => item.AreaDetectorComponent.HandleForceUndetectDetectable(RootDetectable));
    }

    public Rid HandleGetRid()=>GetRid();
}