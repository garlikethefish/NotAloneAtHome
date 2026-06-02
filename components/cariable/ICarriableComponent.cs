namespace NotAloneAtHome.Components;

using System;
using Godot;
public interface ICarriableComponent
{
    CollisionShape2D CollisionShape2D { get; }
    event Action<CarrierComponent> OnPickedUpBy;
    event Action<Vector2> OnDropedAt;
    Func<CarrierComponent, bool> CanBeCarriedBy { get; set; }
    void HandlePickedUpBy(CarrierComponent carrier);
    void HandleDropedAt(Vector2 landPos);
}

