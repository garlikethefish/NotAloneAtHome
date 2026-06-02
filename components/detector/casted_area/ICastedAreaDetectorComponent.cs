namespace NotAloneAtHome.Components;

using System;

#nullable enable
public interface ICastedAreaDetectorComponent : IAreaDetectorComponent
{
    DetectableComponent? ClosestDetectable { get; }
    event Action<DetectableComponent> OnSightEnter;
    event Action<DetectableComponent> OnSightExit;
}