namespace NotAloneAtHome.Tasks;

using Godot;

public partial class WaterPlantsTask
{
    public class WaterGreenPlantStep : ITaskStep<WaterPlantsTask>
    {
        public string Name => "Water the green plant";

        public SceneTree Ctx => Task.Context;

        public WaterPlantsTask Task { get; }

        public WaterGreenPlantStep(WaterPlantsTask task)
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