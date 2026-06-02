namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
#nullable enable

public class DetectableComponentModel
{
    public required DetectableComponent Detectable;
    public bool IsInLineOfSight;
}

public partial class DetectableComponent : StaticBody2D, IDetectableComponent
{
    [Export] public CollisionShape2D CollisionShape2D { get; private set; } = default!;
    public List<AreaDetectorComponent> BlacklistedDetectors { get; } = new();
    public Func<AreaDetectorComponent, bool> CanBeDetected { get; set; } = _ => true;
    public List<AreaDetectorComponent> Detectors = [];
    public List<AreaDetectorComponent> SimpDetectors = []; // Detectors that have this detectable as priority
    public event Action<AreaDetectorComponent>? OnEnteredDetectorArea;
    public event Action<AreaDetectorComponent>? OnExitedDetectorArea;
    public event Action<AreaDetectorComponent>? OnBecameDetectorPriority;
    public event Action<AreaDetectorComponent>? OnRemovedDetectorPriority;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _ExitTree()
    {
        HandleExitAllDetectors();
    }

    public void HandleEnterDetectorArea(AreaDetectorComponent detector)
    {
        if (Detectors.Contains(detector) || BlacklistedDetectors.Contains(detector)) return;
        Detectors.Add(detector);
        OnEnteredDetectorArea?.Invoke(detector);
    }

    public void HandleExitDetectorArea(AreaDetectorComponent detector)
    {
        if (!Detectors.Contains(detector)) return;
        Detectors.Remove(detector);
        OnExitedDetectorArea?.Invoke(detector);
    }

    public void HandleSetAsDetectorPriority(AreaDetectorComponent detector)
    {
        if (SimpDetectors.Contains(detector)) return;
        SimpDetectors.Add(detector);
        OnBecameDetectorPriority?.Invoke(detector);
    }

    public void HandleRemovedFromDetectorPriority(AreaDetectorComponent detector)
    {
        if (!SimpDetectors.Contains(detector)) return;
        SimpDetectors.Remove(detector);
        OnRemovedDetectorPriority?.Invoke(detector);
    }

    public void HandleAddToBlacklist(AreaDetectorComponent detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.HandleBlacklistDetectable(this);
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void HandleRemoveFromBlacklist(AreaDetectorComponent detector)
    {
        BlacklistedDetectors.Remove(detector);
        detector.ExcludedRids.Remove(GetRid());
        detector.HandleAttemptToEnterArea(this);
    }

    public void HandleExitAllDetectors()
    {
        Detectors.ForEach(item => item.HandleForceUndetectDetectable(this));
        SimpDetectors.ForEach(item => item.HandleForceUndetectDetectable(this));
    }

    public Rid HandleGetRid()=>GetRid();
}