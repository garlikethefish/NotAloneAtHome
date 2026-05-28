using Godot;

#nullable enable
public interface ICarrierComponent
{
    Node2D CarryPointNode { get; }
    ICarriable? Carriable { get; }
    void Pickup(ICarriable carriable);
    void Drop();
}