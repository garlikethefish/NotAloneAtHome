namespace NotAloneAtHome.Characters;

using Godot;
using NotAloneAtHome.Components;

[Scene]
public partial class CatFood : Node2D
{
    [Node("DetectableComponent")] private DetectableComponent _detectableComponent;
    [Node("Sprite2D")] private Sprite2D _sprite;

    public override void _Ready()
    {
    }
}