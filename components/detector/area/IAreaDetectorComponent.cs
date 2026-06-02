namespace NotAloneAtHome.Components;

using System;
using System.Collections.Generic;
using Godot;

public interface IAreaDetectorComponent
{
    List<Rid> ExcludedRids { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape2D { get; }
    event Action<DetectableComponent> OnBodyEntered;
    event Action<DetectableComponent> OnBodyExited;
    void HandleBlacklistDetectable(DetectableComponent detectable);
    void HandleForceUndetectDetectable(DetectableComponent detectable);
    void HandleAttemptToEnterArea(DetectableComponent detectable);
}
