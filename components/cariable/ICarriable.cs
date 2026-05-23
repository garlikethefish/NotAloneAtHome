using Godot;
public interface ICarriable : IComponentInterface
{
    CollisionShape2D CollisionShape2D { get; }
    void WhenPickedUpBy(ICarrier carrier);
    void WhenDropedAt(Vector2 landPos);
    bool CanBeCarried(ICarrier carrier);
}

