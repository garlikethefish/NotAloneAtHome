namespace NotAloneAtHome.Components;

public interface ICastedAreaDetector : IAreaDetector
{
    ICastedAreaDetectorComponent CastedAreaDetectorComponent { get; }
    IAreaDetectorComponent IAreaDetector.AreaDetectorComponent => CastedAreaDetectorComponent;
}