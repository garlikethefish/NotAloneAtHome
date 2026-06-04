namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Objects;

#nullable enable
public partial class HideDeadBanditTask
{
    public class PickUpBanditStep(HideDeadBanditTask task) : TaskStepBase(task), ITaskStep<HideDeadBanditTask>
    {
        public new HideDeadBanditTask Task { get; private set; } = task;
        DeadBandit? bandit;

        public override void OnStart()
        {
            UpdateName("Pick up sleepin bandit");
            Log(string.Join(",", Ctx.GetNodesInGroup("task_hide_dead_bandit").Select(item => item.Name)));
            bandit = Ctx.GetNodesInGroup("task_hide_dead_bandit").OfType<DeadBandit>().FirstOrDefault();

            foreach (var node in Ctx.GetNodesInGroup("task_hide_dead_bandit"))
            {
                Log($"{node.Name} - {node.GetType().FullName}");
            }

            if (bandit == null)
            {
                Log("Didnt find DeadBandit!");
                return;
            }

            if (bandit.HasChild<CarriableComponent>(out var carriable))
            {
                carriable.OnPickedUpBy += OnPickedUpBy;
                carriable.OnDropedAt += OnDropedAt;
            }
        }

        public override void OnEnd()
        {
            
        }

        void OnDropedAt(Vector2 pos)
        {
            GoStepBack();
        }

        void OnPickedUpBy(CarrierComponent carrier)
        {
            GoStepForward();
        }
    }
}