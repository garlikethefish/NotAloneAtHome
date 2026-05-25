using Godot;

public interface IThrower : IComponentInterface
{
    Vector2 FacingDirection { get; }
    bool IsAiming { get; }
    void StartAiming();
    void StopAiming();
    void Throw(IThrowable throwable);
    void SetFacingDirection(Vector2 direction);
}