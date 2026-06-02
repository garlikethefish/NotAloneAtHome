namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IThrowableComponent
{
    event Action<ThrowerComponent, Vector2> OnThrownBy;
    event Action<Vector2> OnLanded;
    void HandleThrownBy(ThrowerComponent thrower, Vector2 toPosition);
}

