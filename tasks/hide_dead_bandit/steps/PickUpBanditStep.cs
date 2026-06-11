namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Objects;

public partial class HideDeadBanditTask
{
    public class PickUpBanditStep(HideDeadBanditTask task) : TaskStepBase(task), ITaskStep<HideDeadBanditTask>
    {
        public new HideDeadBanditTask Task { get; private set; } = task;
        private CarriableComponent _banditCarriable;

        public override void OnStart()
        {
            UpdateName("Pick up sleepin bandit");
            if (Task.bandit?.HasChild(out _banditCarriable) == false) return;
            _banditCarriable.OnPickedUpBy += OnPickedUpBy;
        }

        public override void OnStepEnd()
        {
            
        }

        public override void OnTaskEnd()
        {
            _banditCarriable.OnPickedUpBy -= OnPickedUpBy;
            _banditCarriable.OnDropedAt -= OnDropedAt;
        }

        void OnDropedAt(Vector2 pos)
        {
            _banditCarriable.OnDropedAt -= OnDropedAt;
            GoStepBack();
        }

        void OnPickedUpBy(CarrierComponent carrier)
        {
            _banditCarriable.OnPickedUpBy -= OnPickedUpBy;
            _banditCarriable.OnDropedAt += OnDropedAt;
            GoStepForward();
        }
    }
}