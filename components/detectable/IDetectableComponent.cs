namespace NotAloneAtHome.Components.Detectable;

using System.Collections.Generic;
using Godot;

public interface IDetectableComponent : IComponentInterface
{
    CollisionShape2D CollisionShape2D { get; } 
    Rid Rid { get; }
    public List<IAreaDetector> BlacklistedDetectors { get; }
    public void WhenEnteredDetectorArea(IAreaDetector detector);
    public void WhenExitedDetectorArea(IAreaDetector detector);
    public void WhenSetAsDetectorPriority(IAreaDetector detector);
    public void WhenRemovedFromDetectorPriority(IAreaDetector detector);
    public void AddToBlacklist(IAreaDetector detector);
    public void RemoveFromBlacklist(IAreaDetector detector);
    public bool IsDetectorBlacklisted(IAreaDetector detector);
    public bool CanBeDetected(IAreaDetector detector);
    void ExitAllDetectors();
}
