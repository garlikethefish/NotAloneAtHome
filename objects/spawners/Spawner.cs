namespace NotAloneAtHome.Components;

using System;
using Godot;

public partial class Spawner : Node2D
{
    [Node] public SpawnerComponent SpawnerComponent;
    public Action<Node2D> OnSpawned { get; set; }

    public Node2D Spawn(PackedScene scene, Node parentNode = null, Vector2? globalPos = null)
    {
        var instance = SpawnerComponent.HandleSpawn(scene, parentNode, globalPos);
        OnSpawned?.Invoke(instance);
        return instance;
    }
}