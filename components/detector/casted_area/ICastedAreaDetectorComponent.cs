namespace NotAloneAtHome.Components;

using System;

#nullable enable
public interface ICastedAreaDetectorComponent : IAreaDetectorComponent
{
    IDetectable? ClosestDetectable { get; }
}