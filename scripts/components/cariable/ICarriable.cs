using Godot;
public interface ICarriable : IComponentInterface
{
    CollisionShape2D CollisionShape2D { get; }
    void PickUpBy(ICarrier carrier);
    void DropAt(Vector2 landPos);
    bool CanBeCarried(ICarrier carrier);
}

