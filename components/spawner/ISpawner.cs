using System;
using Godot;

public interface ISpawner
{
    Action<Node2D> OnSpawned { get; set; } 
    Node2D Spawn(PackedScene scene, Node parentNode = null, Vector2? globalPos = null);
}