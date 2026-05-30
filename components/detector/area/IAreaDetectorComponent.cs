namespace NotAloneAtHome.Components;

using System;
using System.Collections.Generic;
using Godot;

public interface IAreaDetectorComponent
{
    List<Rid> ExcludedRids { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    event Action<Node2D> OnBodyEntered;
    event Action<Node2D> OnBodyExited;
    void HandleBlacklistDetectable(IDetectable detectable);
    void HandleForceUndetectDetectable(IDetectable detectable);
}
