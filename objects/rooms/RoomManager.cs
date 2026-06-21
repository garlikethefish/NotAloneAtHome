using Godot;
using Godot.Collections;
using NotAloneAtHome.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
public partial class RoomManager : Node
{
    [Export] Array<Room> rooms = [];
    public static RoomManager Instance { get; private set; } = default!;
    public List<Room> RoomsPlayerIsIn = [];

	public override void _Ready()
    {
        Instance = this;
        foreach (var room in rooms)
        {
            room.OnBodyEntered += body => HandleRoomEnter(body, room);
            room.OnBodyExited  += body => HandleRoomExit(body, room);
        }
    }

    void HandleRoomEnter(Node2D body, Room room)
    {
        if (body is Player)
        {
            RoomsPlayerIsIn.Add(room);
        }

        // if (room.IsLightOn)
        // {
        //     foreach (var adjecentRoom in rooms)
        //     {
                
        //     }
        // }
    }

    void HandleRoomExit(Node2D body, Room room)
    {
        if (body is Player)
        {
            RoomsPlayerIsIn.Remove(room);
            GD.Print("Exited room: ", room.Name);
        }   
    }
}
