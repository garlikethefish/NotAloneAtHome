namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IThrowableComponent
{
    event Action<IThrower, Vector2> OnThrownBy;
    event Action<Vector2> OnLanded;
    void HandleThrownBy(IThrower thrower, Vector2 toPosition);
}

