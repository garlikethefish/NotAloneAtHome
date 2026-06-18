using Godot;
using System;

public partial class RoomLightContainer : Node2D
{
    [Export] Rooms RoomName = Rooms.None;
	
	public override void _Ready()
    {
        AddToGroup(RoomName.ToString());
    }
}
