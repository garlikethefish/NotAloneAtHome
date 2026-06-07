namespace NotAloneAtHome.Valuables;

using System;
using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Scripts.Globals;

[Scene]
public partial class Valuable : RigidBody2D
{
    [Export] public ValuableType Type;
    [Export] public Sprite2D Sprite;
    [Node] public AnimationPlayer AnimationPlayer;
    [Node] public DetectableComponent DetectableComponent;
    public double value;
    public event Action OnSold;
    private bool _isSelling;
    // private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");

    public override void _Ready()
    {
        Sprite.Texture = ValuableData.Valuables[Type].texture2D;
        value = ValuableData.Valuables[Type].StealValue;
        // Sprite.Material = _shaderMat;
    }

    public void Sell()
    {
        if (_isSelling) return;
        _isSelling = true;
        
        DetectableComponent.HandleExitAllDetectors();
        DetectableComponent.CustomCanBeDetectedBy = _ => false;

        AnimationPlayer.Play("sell");
        AnimationPlayer.AnimationFinished += _ => {
            GameManager.Instance.StoleItem(this);
            OnSold?.Invoke();
            QueueFree();
        };
    }
}