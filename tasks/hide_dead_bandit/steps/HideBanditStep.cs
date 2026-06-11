namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Characters.DeadThiefCloset;
using NotAloneAtHome.Components;

public partial class HideDeadBanditTask
{
    public class HideBanditStep(HideDeadBanditTask task) : TaskStepBase(task), ITaskStep<HideDeadBanditTask>
    {
        public new HideDeadBanditTask Task { get; private set; } = task;
        private InteractableComponent _closetInteractable;

        public override void OnStart()
        {
            UpdateName("Put thief into closet");
            if (!Task.closet.HasChild(out _closetInteractable))
            {
                Log("Closet doesnt have Interactable");
            }

            _closetInteractable.OnInteractionFrom += OnInteractionFrom;
        }

        public override void OnStepEnd()
        {
            _closetInteractable.OnInteractionFrom -= OnInteractionFrom;
        }

        public override void OnTaskEnd()
        {
            _closetInteractable.OnInteractionFrom -= OnInteractionFrom;
            Task.bandit.PlayShoweThatFatFuckIntoGrannysClosetAnimation(Task.closet);
        }

        void OnInteractionFrom(InteractorComponent interactor)
        {
            GoStepForward();
        }
    }
}