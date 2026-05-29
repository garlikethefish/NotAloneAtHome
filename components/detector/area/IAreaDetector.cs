using System.Collections.Generic;
using Godot;

public interface IAreaDetector 
{
    List<Rid> ExcludedRids { get; }
    List<DetectableComponentModel> DetectablesInArea { get; }
    CollisionShape2D CollisionShape { get; }
    void WhenBodyEntered(Node2D body);
    void WhenBodyExited(Node2D body);
    void BlacklistDetectable(IDetectable detectable);
    void ExitDetectable(IDetectable detectable);
    void ExcludeRid(Rid rid);
    void IncludeRid(Rid rid);
}
