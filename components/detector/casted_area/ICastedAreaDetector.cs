
#nullable enable
using System;

public interface ICastedAreaDetector : IAreaDetector
{
    void WhenEnteredSight(IDetectable detectable);
    void WhenExitedSight(IDetectable detectable);
    IDetectable? ClosestDetectable { get; }
}