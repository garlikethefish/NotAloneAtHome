using Godot;
using System;

public partial class MirrorCamera : Camera2D
{
    [Export] private Camera2D _cameraToMirror; 
    [Export] private SubViewport _subvp;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        GlobalPosition = _cameraToMirror.GlobalPosition;
        Zoom = _cameraToMirror.Zoom;
        _subvp.Size = (Vector2I)GetViewport().GetTexture().GetSize();
	}
}
