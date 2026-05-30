namespace NotAloneAtHome.Components;

using Godot;

#nullable enable
public interface ICarrier 
{
    ICarrierComponent CarrierComponent { get; }
    void Pickup(ICarriable carriable);
    void Drop();
}