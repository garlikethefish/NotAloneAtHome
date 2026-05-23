using Godot;

#nullable enable
public interface ICarrier : IComponentInterface
{
    Node2D CarryPointNode { get; }
    CariableComponent? Carriable { get; }
    void Pickup(ICarriable carriable);
    void Drop();
}