namespace NotAloneAtHome.Valuables;

using Godot;
using NotAloneAtHome.Components;

[Scene]
public partial class Valuable : RigidBody2D
{
    [Export] public ValuableType Type;
    [Export] public Sprite2D Sprite;
    [Node] public AnimationPlayer AnimationPlayer;
    [Node] public DetectableComponent DetectableComponent;
    // private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");

    public override void _Ready()
    {
        Sprite.Texture = ValuableData.Valuables[Type].texture2D;
        // Sprite.Material = _shaderMat;
    }

    public void Sell()
    {
        GD.Print("Sold!");
        AnimationPlayer.Play("sell");
        AnimationPlayer.AnimationFinished += _ => QueueFree();
        DetectableComponent.CanBeDetectedBy = _ => false;
    }
}