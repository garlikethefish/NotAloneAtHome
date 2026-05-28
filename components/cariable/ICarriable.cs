using Godot;
public interface ICarriable
{
    CollisionShape2D CollisionShape2D { get; }
    void OnCarriablePickedUpBy(ICarrier carrier);
    void OnCarriableDropedAt(Vector2 landPos);
    bool CanBeCarried(ICarrier carrier);
}

