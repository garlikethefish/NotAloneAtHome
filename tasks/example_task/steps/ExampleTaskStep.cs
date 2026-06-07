namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Components;

public partial class ExampleTask
{
    public class ExampleTaskStep(ExampleTask task) : TaskStepBase(task), ITaskStep<ExampleTask>
    {
        public new ExampleTask Task { get; private set; } = task;

        public override void OnStart()
        {
            UpdateName($"Set Task step name");
        }

        public override void OnStepEnd()
        {
 
        }

        public override void OnTaskEnd()
        {
            
        }
    }
}