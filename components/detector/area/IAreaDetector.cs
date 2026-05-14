using Godot;

public interface IAreaDetector : IComponentInterface
{
    bool CanDetectLike(DetectableComponent detectable);
}
