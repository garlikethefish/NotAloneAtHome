namespace NotAloneAtHome.Scripts;

using Godot;
using NotAloneAtHome.Tasks;
using NotAloneAtHome.Tasks.WaterPlantsTask;

public partial class SomeNode : Node
{
    public override void _Ready()
    {
        Node mainScene = new Node();
        TaskManager.Instance.AddTask(new WaterPlantsTask(mainScene));
    }
}