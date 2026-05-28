using System.Collections.Generic;
using Godot;

public interface IAreaDetectorComponent
{
    IAreaDetector RootDetector { get; }
    CollisionObject2D[] ExcludedColliders { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    void OnBodyEntered(Node2D body);
    void OnBodyExited(Node2D body);
    void WhenBlacklistedFromDetectable(IDetectable detectable);
    void RemoveDetectable(IDetectable detectable);
}
