using Godot;

public partial class Spawner : ComponentNode2D, ISpawner
{
    public Node SpawnedScene { get; private set; }
    public PackedScene PackedScene { get; set; }

    public Node2D Spawn(PackedScene scene)
    {
        var instance = scene.Instantiate<Node2D>();

        GetTree().CurrentScene.AddChild(instance);

        instance.ProcessMode    = ProcessModeEnum.Always;
        instance.Visible        = true;
        instance.GlobalPosition = GlobalPosition;

        SpawnedScene = instance;

        // var spawnable 
        // = instance.FindChild("*", true, false) as ISpawnable
        //              ?? GetSpawnableFromChildren(instance);

        (instance as ISpawnable)?.OnSpawn(this);

        return instance;
    }

    // private static ISpawnable GetSpawnableFromChildren(Node node)
    // {
    //     foreach (var child in node.GetChildren())
    //     {
    //         if (child is ISpawnable s) return s;
    //         var found = GetSpawnableFromChildren(child);
    //         if (found != null) return found;
    //     }
    //     return null;
    // }
}