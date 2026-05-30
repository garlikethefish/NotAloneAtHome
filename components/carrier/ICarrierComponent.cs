namespace NotAloneAtHome.Components;

using Godot;
#nullable enable
public interface ICarrierComponent
{
    Node2D CarryPointNode { get; }
    ICarriable? Carriable { get; }
    void HandlePickup(ICarriable carriable);
    void HandleDrop();
}