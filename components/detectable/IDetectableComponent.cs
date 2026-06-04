namespace NotAloneAtHome.Components;

using System;
using System.Collections.Generic;
using Godot;

public interface IDetectableComponent 
{
    CollisionShape2D CollisionShape2D { get; } 
    List<AreaDetectorComponent> BlacklistedDetectors { get; }
    event Action<AreaDetectorComponent> OnEnteredDetectorArea;
    event Action<AreaDetectorComponent> OnExitedDetectorArea;
    event Action<AreaDetectorComponent> OnBecameDetectorPriority;
    event Action<AreaDetectorComponent> OnRemovedDetectorPriority;
    Func<AreaDetectorComponent, bool> CanBeDetectedBy { get; set; }
    void HandleEnterDetectorArea(AreaDetectorComponent detector);
    void HandleExitDetectorArea(AreaDetectorComponent detector);
    void HandleSetAsDetectorPriority(AreaDetectorComponent detector);
    void HandleRemovedFromDetectorPriority(AreaDetectorComponent detector);
    void HandleBlacklistDetector(AreaDetectorComponent detector);
    void HandleUnblacklistDetector(AreaDetectorComponent detector);
    void HandleExitAllDetectors();
    Rid HandleGetRid();
}
