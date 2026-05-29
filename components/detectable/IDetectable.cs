

using System;
using System.Collections.Generic;
using Godot;
using NotAloneAtHome.Components.Detectable;

public interface IDetectable
{
    IDetectableComponent DetectableComp { get; }
    Rid Rid { get; }
    List<IAreaDetector> BlacklistedDetectors { get; }
    CollisionShape2D CollisionShape2D { get; }
    Action<IAreaDetector> OnEnteredDetectorArea { get; set; }
    Action<IAreaDetector> OnExitedDetectorArea { get; set; }
    Action<IAreaDetector> OnBecameDetectorPriority { get; set; }
    Action<IAreaDetector> OnRemovedDetectorPriority { get; set; }
    bool CanBeDetected(IAreaDetector detector);
    void ExitAllDetectors();
}
