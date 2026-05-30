namespace NotAloneAtHome.Components.Detectable;

using System.Collections.Generic;
using Godot;

public interface IDetectableComponent : IComponentInterface
{
    CollisionShape2D CollisionShape2D { get; } 
    ReactiveList<IAreaDetector> BlacklistedDetectors { get; }
    void HandleEnterDetectorArea(IAreaDetector detector);
    void HandleExitDetectorArea(IAreaDetector detector);
    void HandleSetAsDetectorPriority(IAreaDetector detector);
    void HandleRemovedFromDetectorPriority(IAreaDetector detector);
    void HandleAddToBlacklist(IAreaDetector detector);
    void HandleRemoveFromBlacklist(IAreaDetector detector);
    void HandleExitAllDetectors();
    Rid HandleGetRid();
}
