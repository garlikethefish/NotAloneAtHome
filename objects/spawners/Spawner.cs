namespace NotAloneAtHome.Components;

using System;
using Godot;

public partial class Spawner : Node2D, ISpawner
{
    public ComponentHolder Holder { get; set; }
    private ISpawnerComponent _spawnerComp;
    public Action<Node2D> OnSpawned { get; set; }

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        _spawnerComp = Holder.SpawnerComp;
    }

    public Node2D Spawn(PackedScene scene, Node parentNode = null, Vector2? globalPos = null)
    {
        var instance = _spawnerComp.HandleSpawn(scene, parentNode, globalPos);
        OnSpawned?.InvokeOrLog(instance);
        return instance;
    }
}