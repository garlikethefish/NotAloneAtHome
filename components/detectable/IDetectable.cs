

using System.Collections.Generic;

public interface IDetectable : IComponentInterface
{
    public List<IAreaDetector> BlacklistedDetectors { get; }
    public void WhenEnteredDetectorArea(IAreaDetector detector);
    public void WhenExitedDetectorArea(IAreaDetector detector);
    public void WhenSetAsDetectorPriority(IAreaDetector detector);
    public void WhenRemovedFromDetectorPriority(IAreaDetector detector);
    public void AddToBlacklist(IAreaDetector detector);
    public void RemoveFromBlacklist(IAreaDetector detector);
    public bool IsDetectorBlacklisted(IAreaDetector detector);
    public bool CanBeDetected(IAreaDetector detector);
}
