namespace NotAloneAtHome.Components;
using Godot;

public interface ISpawnerComponent
{
    Node2D HandleSpawn(PackedScene scene, Node parentNode = null, Vector2? globalPos = null);
}