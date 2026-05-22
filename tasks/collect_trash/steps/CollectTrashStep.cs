namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using Godot;
using NotAloneAtHome.Tasks.Interfaces;

public partial class CollectTrashTask
{
    public class CollectTrashStep : ITaskStep<CollectTrashTask>
    {
        public string Name => "Pick up trash";

        public Node Context => Task.Context;

        public CollectTrashTask Task { get; }

        public CollectTrashStep(CollectTrashTask task)
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
            // spawn in and get refs to all trash
            
        }

        public void End() {}

        public void Finish()
        {
            
            EmitNext();
        }
    }
}