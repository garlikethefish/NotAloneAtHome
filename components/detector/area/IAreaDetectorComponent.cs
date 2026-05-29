using System.Collections.Generic;
using Godot;

public interface IAreaDetectorComponent
{
    List<Rid> ExcludedRids { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    void OnBodyEntered(Node2D body);
    void OnBodyExited(Node2D body);
    void HandleBlacklistDetectable(IDetectable detectable);
    void HandleExitDetectable(IDetectable detectable);
    void HandleExcludeRid(Rid rid);
    void HandleIncludeRid(Rid rid);
}
