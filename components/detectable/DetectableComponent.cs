namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

public class DetectableComponentModel
{
    public required DetectableComponent Detectable;
    public bool IsInLineOfSight;
}

public partial class DetectableComponent : AnimatableBody2D, IDetectableComponent
{
    public List<AreaDetectorComponent> BlacklistedDetectors { get; } = [];
    public Func<AreaDetectorComponent, bool> CustomCanBeDetectedBy { get; set; } = _ => true;
    private bool _isDetectable = true;
    public bool IsDetectable
    {
        get => _isDetectable;
        set
        {
            _isDetectable = value;
            if (!value)
                HandleExitAllDetectors();
        }
    }
    public List<AreaDetectorComponent> Detectors = [];
    public List<AreaDetectorComponent> SimpDetectors = []; // Detectors that have this detectable as priority
    private Vector2 _startingPosition; // FUCK GODOT
    public event Action<AreaDetectorComponent>? OnEnteredDetectorArea;
    public event Action<AreaDetectorComponent>? OnExitedDetectorArea;
    public event Action<AreaDetectorComponent>? OnBecameDetectorPriority;
    public event Action<AreaDetectorComponent>? OnRemovedDetectorPriority;

    public override void _Ready()
    {
        base._Ready();
        _startingPosition = Position; // FUCK GODOT

        if (this.ParentHas<AreaDetectorComponent>(out var areaDetector))
        {
            HandleBlacklistDetector(areaDetector);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = _startingPosition; // FUCK GODOT
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            HandleExitAllDetectors();
        }
    }

    public bool CanDetect(AreaDetectorComponent detector) => _isDetectable && CustomCanBeDetectedBy(detector);

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

    public void HandleBlacklistDetector(AreaDetectorComponent detector)
    {
        BlacklistedDetectors.Add(detector);
        detector.HandleUndetectDetectable(this);
        detector.ExcludedRids.Add(HandleGetRid());
        SimpDetectors.Remove(detector);
        Detectors.Remove(detector);
    }

    public void HandleUnblacklistDetector(AreaDetectorComponent detector)
    {
        BlacklistedDetectors.Remove(detector);
        detector.ExcludedRids.Remove(GetRid());
        detector.HandleAttemptToEnterArea(this);
    }

    public void HandleExitAllDetectors()
    {
        Detectors.ToList().ForEach(item => item.HandleUndetectDetectable(this));
        SimpDetectors.ToList().ForEach(item => item.HandleUndetectDetectable(this));
    }

    public Rid HandleGetRid()=>GetRid();
}