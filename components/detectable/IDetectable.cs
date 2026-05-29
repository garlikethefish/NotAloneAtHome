

using System;
using System.Collections.Generic;
using Godot;

public interface IDetectable
{
    event Action<IAreaDetector> OnDetectableBecamePriority;
    event Action<IAreaDetector> OnDetectableLostPriority;
    Rid Rid { get; }
    List<IAreaDetector> BlacklistedDetectors { get; }
    CollisionShape2D CollisionShape2D { get; }
    void WhenEnteredDetectorArea(IAreaDetector detector);
    void WhenExitedDetectorArea(IAreaDetector detector);
    void WhenSetAsDetectorPriority(IAreaDetector detector);
    void WhenRemovedFromDetectorPriority(IAreaDetector detector);
    void AddToDetectorBlacklist(IAreaDetector detector);
    void RemoveFromDetectorBlacklist(IAreaDetector detector);
    bool IsDetectorBlacklisted(IAreaDetector detector);
    bool CanBeDetected(IAreaDetector detector);
    void ExitAllDetectors();
}
