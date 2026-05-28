using Godot;
public interface ICarriableComponent
{
    void OnPickedUpBy(ICarrier carrier);
    void OnDropedAt(Vector2 landPos);
    bool CanBeCarried(ICarrier carrier);
}

