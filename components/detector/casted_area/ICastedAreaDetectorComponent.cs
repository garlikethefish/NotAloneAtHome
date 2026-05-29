
#nullable enable
using System;

public interface ICastedAreaDetectorComponent : IAreaDetectorComponent
{
    IDetectable? ClosestDetectable { get; }
}