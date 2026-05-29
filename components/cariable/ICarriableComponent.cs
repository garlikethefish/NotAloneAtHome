using Godot;
public interface ICarriableComponent
{
    void HandlePickedUpBy(ICarrier carrier);
    void HandleDropedAt(Vector2 landPos);
    bool CanBeCarried(ICarrier carrier);
}

