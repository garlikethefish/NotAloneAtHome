using Godot;
using NotAloneAtHome.Components;
using System;

[Scene]
public partial class LightSwitch : Node2D
{
    [Export] private Room room;
    [Export] private bool useSideTextures = false;
    [Export] private bool flipSideSpriteHorizontally = false;

    [Node("InteractableComponent")]
    private InteractableComponent _interactable = default!;

    [Node("Sprite2D")]
    private Sprite2D _sprite;
    private Texture2D _offFront = GD.Load<Texture2D>("res://sprites/lightswitch_front_off.png");
    private Texture2D _onFront = GD.Load<Texture2D>("res://sprites/lightswitch_front_on.png");
    private Texture2D _offSide = GD.Load<Texture2D>("res://sprites/lightswitch_side_off.png");
    private Texture2D _onSide = GD.Load<Texture2D>("res://sprites/lightswitch_side_on.png");

    public override void _Ready()
    {
        _interactable.OnInteractionFrom += ToggleLight;
        UpdateSprite();
    }

    private void ToggleLight(InteractorComponent interactor)
    {
        room.ToggleLight();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        bool isOn = room.IsLightOn;

        if (useSideTextures)
        {
            _sprite.Texture = isOn ? _onSide : _offSide;

            // Apply flip only for side mode
            _sprite.FlipH = flipSideSpriteHorizontally;
        }
        else
        {
            _sprite.Texture = isOn ? _onFront : _offFront;

            // Reset flip for front mode so it doesn't carry over
            _sprite.FlipH = false;
        }
    }
}