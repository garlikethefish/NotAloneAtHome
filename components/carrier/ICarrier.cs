using Godot;

public interface ICarrier: IComponentInterface
{
    Node2D CarryPointNode { get; }
    void Pickup(ICarriable carriable);
    void Drop();
}