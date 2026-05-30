namespace NotAloneAtHome.Components;

using System;
using Godot;
public interface ICarriable
{
    ICarriableComponent CarriableComponent { get; }
    bool CanBeCarried(ICarrier carrier);
}

