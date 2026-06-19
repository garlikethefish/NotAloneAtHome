using Godot;
using System;

public partial class MirrorCamera : Camera2D
{
    [Export] private Camera2D _cameraToMirror; 

    public override void _Ready()
    {
        ProcessPriority = 1;
    }
    
	public override void _PhysicsProcess(double delta)
    {   
        Position = _cameraToMirror.Position;
        GlobalPosition = _cameraToMirror.GlobalPosition;
        Zoom = _cameraToMirror.Zoom;
    }
}
