

using System;
using System.Collections.Generic;
using Godot;

public interface IDetectable
{
    event Action<IAreaDetector> OnDetectableBecamePriority;
    event Action<IAreaDetector> OnDetectableLostPriority;
    Rid Rid { get; }
    public List<IAreaDetector> BlacklistedDetectors { get; }
    public CollisionShape2D CollisionShape2D { get; }
    public void OnDetectableEnteredDetectorArea(IAreaDetector detector);
    public void OnDetectableExitedDetectorArea(IAreaDetector detector);
    public void OnDetectableSetAsDetectorPriority(IAreaDetector detector);
    public void OnDetectableRemovedFromDetectorPriority(IAreaDetector detector);
    public void DetectableAddToBlacklist(IAreaDetector detector);
    public void DetectableRemoveFromBlacklist(IAreaDetector detector);
    public bool DetectableIsDetectorBlacklisted(IAreaDetector detector);
    public bool CanBeDetected(IAreaDetector detector);
    void ExitAllDetectors();
}
