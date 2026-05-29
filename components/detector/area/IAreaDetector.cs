using System;
using System.Collections.Generic;
using Godot;

public interface IAreaDetector 
{
    List<Rid> ExcludedRids { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    Action<Node2D> OnBodyEntered { get; set; }
    Action<Node2D> OnBodyExited { get; set; }
    void BlacklistDetectable(IDetectable detectable);
    void ForceUndetectDetectable(IDetectable detectable);
}
