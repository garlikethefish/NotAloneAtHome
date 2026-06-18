using Godot;
using System;
using NotAloneAtHome.Components;
using Godot.Collections;

[Tool]
public partial class Door : AnimatableBody2D
{
    public enum DoorDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum DoorStyle
    {
        Side,
        Front
    }

    [Export] public DoorDirection OpenDirection = DoorDirection.Right;
    [Export] public DoorStyle Style = DoorStyle.Side;
    [Export] public bool FlipSprite = false;

    [Export] public float OpenDistance = 25;
    [Export] public float Speed = 6f;

    private InteractableComponent _interactable;
    private Sprite2D _sprite;

    private CollisionShape2D _sideShape;
    private CollisionShape2D _frontShape;

    private bool _isOpen;
    private bool _isMoving;

    private Vector2 _closedPos;
    private Vector2 _openPos;

    private float _t;

    public override void _ValidateProperty(Dictionary property)
	{
		if (Engine.IsEditorHint())
            Setup();
	}

    public override void _Ready()
    {
        Setup();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving)
        {
            _t += (float)delta * Speed;
            float p = Mathf.Clamp(_t, 0f, 1f);

            Vector2 target = _isOpen ? _openPos : _closedPos;
            Position = Position.Lerp(target, p);

            if (_t >= 1f)
                _isMoving = false;
        }
    }

    private void HandleInteraction(InteractorComponent interactor)
    {
        if (_isMoving)
            return;

        _isOpen = !_isOpen;
        _t = 0f;
        _isMoving = true;
    }

    // -----------------------------
    // SETUP
    // -----------------------------

    private void Setup()
    {
        _interactable = this.GetChild<InteractableComponent>();
        _sprite = this.GetChild<Sprite2D>();

        _sideShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D_Side");
        _frontShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D_Front");

        if (_interactable != null)
        {
            _interactable.OnInteractionFrom -= HandleInteraction;
            _interactable.OnInteractionFrom += HandleInteraction;
        }

        _closedPos = Position;

        CalculateOpenPosition();
        ApplyVisuals();
        ApplyCollisionMode();
    }

    private void CalculateOpenPosition()
    {
        _closedPos = Position;

        Vector2 dir = OpenDirection switch
        {
            DoorDirection.Up => Vector2.Up,
            DoorDirection.Down => Vector2.Down,
            DoorDirection.Left => Vector2.Left,
            DoorDirection.Right => Vector2.Right,
            _ => Vector2.Right
        };

        _openPos = _closedPos + dir * OpenDistance;
    }

    private void ApplyVisuals()
    {
        if (_sprite == null)
            return;

        _sprite.Texture = ResourceLoader.Load<Texture2D>(
            Style == DoorStyle.Front
                ? "res://sprites/frontdoor.png"
                : "res://sprites/sidedoor.png"
        );

        _sprite.FlipH = FlipSprite;
    }

    // -----------------------------
    // COLLISION SWITCH ONLY
    // -----------------------------

    private void ApplyCollisionMode()
    {
        if (_sideShape != null)
            _sideShape.Disabled = Style != DoorStyle.Side;

        if (_frontShape != null)
            _frontShape.Disabled = Style != DoorStyle.Front;
    }
}