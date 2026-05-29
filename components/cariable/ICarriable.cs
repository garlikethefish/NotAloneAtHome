using System;
using Godot;
public interface ICarriable
{
    CollisionShape2D CollisionShape2D { get; }
    Action<ICarrier> OnPickedUpBy { get; set; }
    Action<Vector2> OnDropedAt { get; set; }
    bool CanBeCarried(ICarrier carrier);
}

