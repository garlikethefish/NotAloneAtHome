namespace NotAloneAtHome.Valuables;

using System;
using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Scripts.Globals;

[Scene]
public partial class Valuable : RigidBody2D
{
    [Export] public Sprite2D Sprite;
    [Export] public double value = 0;
    [Node] public AnimationPlayer AnimationPlayer;
    [Node] public DetectableComponent DetectableComponent;
    public event Action OnSold;
    private bool _isSelling;
    // private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");

    public override void _Ready()
    {
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