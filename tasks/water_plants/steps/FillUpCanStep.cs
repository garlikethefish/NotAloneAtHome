namespace NotAloneAtHome.Tasks;

using Godot;

public partial class WaterPlantsTask
{
    public class FillUpCanStep : ITaskStep<WaterPlantsTask>
    {
        public string Name => "Fill up watering can";

        public SceneTree Ctx => Task.Context;

        public WaterPlantsTask Task { get; }

        public FillUpCanStep(WaterPlantsTask task)
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

        public void Start()
        {
            if (Task.HasFilledUpWateringCan)
            {
                EmitNext();
            }
        }

        public void End() {}

        public void Finish()
        {
            Task.HasFilledUpWateringCan = true;
            EmitNext();
        }
    }
}