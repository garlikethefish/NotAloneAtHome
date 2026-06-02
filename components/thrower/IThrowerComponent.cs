namespace NotAloneAtHome.Components;

using Godot;

public interface IThrowerComponent
{
    Vector2 FacingDirection { get; }
    bool IsAiming { get; }
    void HandleStartAiming();
    void HandleStopAiming();
    void HandleThrow(ThrowableComponent throwable);
}