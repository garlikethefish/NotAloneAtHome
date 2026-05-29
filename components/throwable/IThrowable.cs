using System;
using Godot;

public interface IThrowable
{
    Action<IThrower, Vector2> OnThrownBy { get; set; }
    Action<Vector2> OnLanded { get; set; }
}

