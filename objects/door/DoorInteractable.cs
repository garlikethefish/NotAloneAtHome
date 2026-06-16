using Godot;
using System;
using NotAloneAtHome.Components;

[Tool]
public partial class DoorInteractable : Node2D
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

    [Export] public DoorDirection Direction = DoorDirection.Right;
    [Export] public DoorStyle Style = DoorStyle.Side;

    [Export] public float OpenDistance = 48f;
    [Export] public float Speed = 6f;

    private InteractableComponent _interactable;
    private AnimatableBody2D _doorBody;
    private Sprite2D _sprite;

    private CollisionShape2D _sideShape;
    private CollisionShape2D _frontShape;

    private bool _isOpen;
    private bool _isMoving;

    private Vector2 _closedPos;
    private Vector2 _openPos;

    private float _t;

    public override void _Ready()
    {
        Setup();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            Setup();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_doorBody == null)
            return;

        if (_isMoving)
        {
            _t += (float)delta * Speed;
            float p = Mathf.Clamp(_t, 0f, 1f);

            Vector2 target = _isOpen ? _openPos : _closedPos;
            _doorBody.Position = _doorBody.Position.Lerp(target, p);

            if (_t >= 1f)
                _isMoving = false;
        }
    }

    private void HandleInteraction(InteractorComponent interactor)
    {
        if (_isMoving || _doorBody == null)
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
        _interactable = FindInteractable(this);
        _doorBody = FindDoorBody(this);
        _sprite = FindSprite(this);

        _sideShape = _doorBody?.GetNodeOrNull<CollisionShape2D>("CollisionShape2D_Side");
        _frontShape = _doorBody?.GetNodeOrNull<CollisionShape2D>("CollisionShape2D_Front");

        if (_interactable != null)
        {
            _interactable.OnInteractionFrom -= HandleInteraction;
            _interactable.OnInteractionFrom += HandleInteraction;
        }

        if (_doorBody != null)
            _closedPos = _doorBody.Position;

        CalculateOpenPosition();
        ApplyVisuals();
        ApplyCollisionMode();
    }

    private void CalculateOpenPosition()
    {
        if (_doorBody == null)
            return;

        _closedPos = _doorBody.Position;

        Vector2 dir = Direction switch
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

    // -----------------------------
    // FINDERS
    // -----------------------------

    private InteractableComponent FindInteractable(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is InteractableComponent i)
                return i;

            var r = FindInteractable(child);
            if (r != null)
                return r;
        }
        return null;
    }

    private AnimatableBody2D FindDoorBody(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is AnimatableBody2D b)
                return b;

            var r = FindDoorBody(child);
            if (r != null)
                return r;
        }
        return null;
    }

    private Sprite2D FindSprite(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Sprite2D s)
                return s;

            var r = FindSprite(child);
            if (r != null)
                return r;
        }
        return null;
    }
}