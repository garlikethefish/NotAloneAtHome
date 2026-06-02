namespace NotAloneAtHome.Valuables;

using Godot;
using NotAloneAtHome.Components;

[Scene]
public partial class Valuable : RigidBody2D
{
    [Export] public ValuableType Type;
    [Export] public Sprite2D Sprite;
    private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");
    [Node] public DetectableComponent DetectableComponent;
    [Node] public CarriableComponent CarriableComponent;
    [Node] public ThrowableComponent ThrowableComponent;
    [Node] public InteractableComponent InteractableComponent;

    public override void _Ready()
    {
        WireNodes();
        Sprite.Texture = ValuableData.Valuables[Type].texture2D;
        Sprite.Material = _shaderMat;

        ThrowableComponent.OnThrownBy += ThrownBy;
        CarriableComponent.OnPickedUpBy += PickedUpBy;
        CarriableComponent.OnDropedAt += DropedAt;
    }

    

    public void Sell(Node2D node)
    {
        GD.Print("Sold!");
    }

    void ThrownBy(ThrowerComponent thrower, Vector2 pos)
    {
        
    }

    void PickedUpBy(CarrierComponent carrier)
    {
    }

    void DropedAt(Vector2 pos)
    {
    }

}