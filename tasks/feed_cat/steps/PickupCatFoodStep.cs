namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Characters;   // CatFood lives here
using NotAloneAtHome.Components;

public partial class FeedCatTask
{
    public class PickupCatFoodStep(FeedCatTask task) : TaskStepBase(task), ITaskStep<FeedCatTask>
    {
        public new FeedCatTask Task { get; private set; } = task;

        private CatFood _catFood;
        private CarriableComponent _catFoodCarriable;

        public override void OnStart()
        {
            UpdateName($"Pick up cat food");

            _catFood = Ctx.GetNodeFromGroup<CatFood>("task_feed_cat");

            if (!_catFood.HasChild(out _catFoodCarriable))
            {
                Log("Cat food didnt have carriable component!");
                return;
            }

            _catFoodCarriable.OnPickedUpBy += OnPickupFood;
        }

        public override void OnStepEnd()
        {
        }

        public override void OnTaskEnd()
        {
            _catFoodCarriable.OnPickedUpBy -= OnPickupFood;
            _catFoodCarriable.OnDropedAt -= OnDropFood;

            if (_catFood.HasChild<HealthComponent>(out var health))
            {
                health.TakeDamage(10000000);
            }
        }

        void OnPickupFood(CarrierComponent carrier)
        {
            _catFoodCarriable.OnPickedUpBy -= OnPickupFood;
            _catFoodCarriable.OnDropedAt   += OnDropFood;
            GoStepForward();
        }

        void OnDropFood(Vector2 pos)
        {
            _catFoodCarriable.OnDropedAt -= OnDropFood;
            GoStepBack();
        }
    }
}