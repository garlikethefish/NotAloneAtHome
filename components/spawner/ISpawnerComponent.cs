namespace NotAloneAtHome.Components.Spawner;
using Godot;

public interface ISpawnerComponent
{
    Node2D HandleSpawn(PackedScene scene, Node parentNode = null, Vector2? globalPos = null);
}