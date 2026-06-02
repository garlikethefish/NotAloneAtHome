namespace NotAloneAtHome.Components;

using Godot;

public partial class SpawnerComponent : Node2D, ISpawnerComponent
{
    public Node2D HandleSpawn(PackedScene scene, Node parent = null, Vector2? globalPos = null)
    {
        var instance = scene.Instantiate<Node2D>(); 
        parent ??= GetTree().CurrentScene; 
        parent.AddChild(instance);
        instance.GlobalPosition = globalPos ?? GetParent<Node2D>().GlobalPosition;
        return instance;
    }
}