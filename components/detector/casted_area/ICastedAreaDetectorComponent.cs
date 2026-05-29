
#nullable enable
public interface ICastedAreaDetectorComponent : IAreaDetectorComponent
{
    IDetectable? ClosestDetectable { get; }
    void OnEnteredSight(IDetectable detectable);
    void OnExitedSight(IDetectable detectable);
}