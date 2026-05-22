
#nullable enable
public interface ICastedAreaDetector : IAreaDetector
{
    IDetectable? ClosestDetectable { get; }
}