using Godot;
using System;

public partial class StaticNode2D : Node2D
{
    [Export] Node2D node;
    private Vector2 _startingGlobalPos = Vector2.Zero;
	public override void _Ready()
    {
        _startingGlobalPos = node.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        node.GlobalPosition = _startingGlobalPos;
    }
}
