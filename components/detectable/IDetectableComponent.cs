namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IDetectableComponent : IComponentInterface
{
    CollisionShape2D CollisionShape2D { get; } 
    ReactiveList<IAreaDetector> BlacklistedDetectors { get; }
    event Action<IAreaDetector> OnEnteredDetectorArea;
    event Action<IAreaDetector> OnExitedDetectorArea;
    event Action<IAreaDetector> OnBecameDetectorPriority;
    event Action<IAreaDetector> OnRemovedDetectorPriority;
    void HandleEnterDetectorArea(IAreaDetector detector);
    void HandleExitDetectorArea(IAreaDetector detector);
    void HandleSetAsDetectorPriority(IAreaDetector detector);
    void HandleRemovedFromDetectorPriority(IAreaDetector detector);
    void HandleAddToBlacklist(IAreaDetector detector);
    void HandleRemoveFromBlacklist(IAreaDetector detector);
    void HandleExitAllDetectors();
    Rid HandleGetRid();
}
