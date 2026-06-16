using Godot;
using NotAloneAtHome.Components;
using System;

[Scene]
public partial class LightSwitch : Node2D
{
    [Export] private Room room = default!;

    [Export] private bool useSideTextures = false;

    // New option: flip side sprite horizontally
    [Export] private bool flipSideSprite = false;

    // --- Added property for configurable detection radius ---
    [Export] private float detectableRadius = 50f;

    [Node("InteractableComponent")]
    private InteractableComponent _interactable = default!;

    [Node("DetectableComponent")]
    private DetectableComponent _detectable = default!;

    [Node("Sprite2D")]
    private Sprite2D _sprite = default!;

    private Texture2D _offFront = GD.Load<Texture2D>(
        "res://sprites/lightswitch_front_off.png");

    private Texture2D _onFront = GD.Load<Texture2D>(
        "res://sprites/lightswitch_front_on.png");

    private Texture2D _offSide = GD.Load<Texture2D>(
        "res://sprites/lightswitch_side_off.png");

    private Texture2D _onSide = GD.Load<Texture2D>(
        "res://sprites/lightswitch_side_on.png");

    public override void _Ready()
    {
        base._Ready();

        ApplyDetectableRadius();

        _interactable.OnInteractionFrom += ToggleLight;

        UpdateSprite();
    }

    private void ApplyDetectableRadius()
    {
        // Adjusting the CollisionShape2D radius for this specific component instance
        var collisionShape = _detectable.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collisionShape?.Shape is CircleShape2D circle)
        {
            var uniqueCircle = (CircleShape2D)circle.Duplicate();
            uniqueCircle.Radius = detectableRadius;
            collisionShape.Shape = uniqueCircle;
        }
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
            _sprite.FlipH = flipSideSprite;
        }
        else
        {
            _sprite.Texture = isOn ? _onFront : _offFront;

            // Reset flip for front mode so it doesn't carry over
            _sprite.FlipH = false;
        }
    }
}