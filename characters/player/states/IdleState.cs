namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class IdleState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public IdleState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Update(double delta)
        {
            if (Input.IsActionJustPressed("interact"))
            {
                var detectable = Ctx._detector.ClosestDetectable;
                if (detectable == null) return;

                if (detectable.Root is ICarriable carriable && carriable.CanBeCarried(Ctx))
                {
                    Ctx.Pickup(carriable);
                    Ctx.ChangeState(Ctx.States[typeof(CarryingState)]);
                }
                else if (detectable.Root is IInteractable interactable && interactable.CanBeInteractedBy(Ctx))
                {
                    Ctx.InteractWith(interactable);
                }
            }

            if (Ctx._moveDirection != Vector2.Zero)
            {
                Ctx.ChangeState(Ctx.States[typeof(WalkingState)]);
            }

            if (Input.IsActionJustPressed("toggle_mask"))
            {
                Ctx.ChangeState(Ctx.States[typeof(MaskedState)]);
            }
        }

        public void Enter()
        {
            Ctx.ImmobileAnimation();
        }

        public void Exit() {}

        public void PhysicsUpdate(double delta) {}
    }
}