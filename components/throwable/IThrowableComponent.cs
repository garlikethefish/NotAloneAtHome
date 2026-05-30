using System;
using Godot;

public interface IThrowableComponent
{
    event Action<IThrower, Vector2> OnThrownBy;
    void HandleThrownBy(IThrower thrower, Vector2 toPosition);
}

