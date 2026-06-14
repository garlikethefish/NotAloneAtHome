using Godot;
using System;

public partial class DummyPlayer : CharacterBody2D
{
	public const float Speed = 300.0f;
	public Vector2 _moveDirection = Vector2.Zero;

	public override void _PhysicsProcess(double delta)
	{
		_moveDirection.X = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		_moveDirection.Y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
		_moveDirection = _moveDirection.Normalized();

	
		Velocity = _moveDirection * Speed;
		MoveAndSlide();
	}
}
