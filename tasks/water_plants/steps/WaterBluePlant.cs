namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using Godot;
using NotAloneAtHome.Tasks.Interfaces;

public partial class WaterPlantsTask
{
    public class WaterBluePlantStep : ITaskStep<WaterPlantsTask>
    {
        public string Name => "Water the blue plant";

        public Node Context => Task.Context;

        public WaterPlantsTask Task { get; }

        public WaterBluePlantStep(WaterPlantsTask task)
        {
            Task = task;
        }

        public void EmitNext()
        {
            Task.Next();
        }

        public void EmitBack()
        {
            Task.Back();
        }

        public void Start() {}

        public void End() {}

        public void Finish()
        {
            EmitNext();
        }
    }
}