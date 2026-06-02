namespace NotAloneAtHome.Components;

using Godot;
#nullable enable
public interface ICarrierComponent : INewComponent
{
    Node2D CarryPointNode { get; }
    CarriableComponent? CarriableComp { get; }
    void HandlePickup(CarriableComponent carriable);
    void HandleDrop();
}