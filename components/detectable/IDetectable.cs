namespace NotAloneAtHome.Components;

public interface IDetectable
{
    IDetectableComponent DetectableComponent { get; }
    bool CanBeDetected(IAreaDetector detector);
}
