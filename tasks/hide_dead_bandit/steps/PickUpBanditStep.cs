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
            bandit = Ctx.GetNodesInGroup("task_hide_dead_bandit").OfType<DeadBandit>().FirstOrDefault();

            if (bandit == null)
            {
                Log("Didnt find DeadBandit!");
                return;
            }

            if (bandit.HasChild<CarriableComponent>(out var carriable))
            {
                carriable.OnPickedUpBy += OnPickedUpBy;
            }
        }

        public override void OnStepEnd()
        {
            
        }

        public override void OnTaskEnd()
        {
            if (bandit?.HasChild<CarriableComponent>(out var carriable) == true)
            {
                carriable.OnPickedUpBy -= OnPickedUpBy;
                carriable.OnDropedAt -= OnDropedAt;
            }
        }

        void OnDropedAt(Vector2 pos)
        {
            if (bandit?.HasChild<CarriableComponent>(out var carriable) == true)
            {
                carriable.OnDropedAt -= OnDropedAt;
            }
            GoStepBack();
        }

        void OnPickedUpBy(CarrierComponent carrier)
        {
            if (bandit?.HasChild<CarriableComponent>(out var carriable) == true)
            {
                carriable.OnPickedUpBy -= OnPickedUpBy;
                carriable.OnDropedAt += OnDropedAt;
            }
            GoStepForward();
        }
    }
}