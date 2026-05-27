using System.Collections.Generic;
using Godot;

public interface IAreaDetector : IComponentInterface
{
    CollisionObject2D[] ExcludedColliders { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    bool CanDetectLike(DetectableComponent detectable);
    void OnBodyEntered(Node2D body);
    void OnBodyExited(Node2D body);
    void WhenBlacklistedFromDetectable(IDetectable detectable);
    void RemoveDetectable(IDetectable detectable);
}
