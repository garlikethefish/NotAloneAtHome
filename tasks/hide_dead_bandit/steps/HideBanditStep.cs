namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Characters.DeadThiefCloset;
using NotAloneAtHome.Components;

#nullable enable
public partial class HideDeadBanditTask
{
    public class HideBanditStep(HideDeadBanditTask task) : TaskStepBase(task), ITaskStep<HideDeadBanditTask>
    {
        public new HideDeadBanditTask Task { get; private set; } = task;

        DeadBanditCloset? closet;

        public override void OnStart()
        {
            UpdateName("Put thief into closet");
            closet = Ctx.GetNodesInGroup("task_hide_dead_bandit").OfType<DeadBanditCloset>().FirstOrDefault();

            if (closet == null)
            {
                Log("Didnt find DeadBanditCloset");
                return;
            }

            if (closet.HasChild<InteractableComponent>(out var interactable))
            {
                interactable.OnInteractionFrom += OnInteractionFrom;
            }
        }

        public override void OnEnd()
        {
        }

        void OnInteractionFrom(InteractorComponent interactor)
        {
            GoStepForward();
        }
    }
}