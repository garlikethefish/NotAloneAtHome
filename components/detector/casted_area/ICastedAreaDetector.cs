
#nullable enable
using System;

public interface ICastedAreaDetector : IAreaDetector
{
    
    Action<IDetectable> OnEnteredSight { get; set; }
    Action<IDetectable> OnExitedSight { get; set; }
    IDetectable? ClosestDetectable { get; }
}