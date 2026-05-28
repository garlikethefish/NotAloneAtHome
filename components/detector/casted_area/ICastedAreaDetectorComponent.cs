
#nullable enable
public interface ICastedAreaDetectorComponent : IAreaDetectorComponent
{
    IDetectable? ClosestDetectable { get; }
}