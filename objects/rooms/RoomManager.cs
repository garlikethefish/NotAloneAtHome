using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
public partial class RoomManager : Node2D
{
    [Export] Array<Room> rooms = [];
    public static RoomManager Instance { get; private set; } = default!;
    public event Action<Node2D, Room>? OnRoomEntered;
    public event Action<Room>? OnRoomUpdated;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        Instance = this;
        foreach (var room in rooms)
        {
            room.OnBodyEntered += body => OnRoomEntered?.Invoke(body, room);
            room.OnRoomUpdated += () => OnRoomUpdated?.Invoke(room);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public Room? FindRoomWhereIsNode(Node2D node)
    {
        return null; //rooms.FirstOrDefault(area => area.GetOverlappingBodies().Contains(node));
    }
}
