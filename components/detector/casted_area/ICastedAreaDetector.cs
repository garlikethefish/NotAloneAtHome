
#nullable enable
using System;

public interface ICastedAreaDetector : IAreaDetector
{
    void OnEnteredSight(IDetectable detectable);
    void OnExitedSight(IDetectable detectable);
    IDetectable? ClosestDetectable { get; }
}