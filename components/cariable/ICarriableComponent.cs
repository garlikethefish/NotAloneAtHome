namespace NotAloneAtHome.Components;

using System;
using Godot;
public interface ICarriableComponent
{
    CollisionShape2D CollisionShape2D { get; }
    event Action<ICarrier> OnPickedUpBy;
    event Action<Vector2> OnDropedAt;
    void HandlePickedUpBy(ICarrier carrier);
    void HandleDropedAt(Vector2 landPos);
}

