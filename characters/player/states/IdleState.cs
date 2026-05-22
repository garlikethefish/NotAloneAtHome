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
                
                if (detectable is ICarriable carriable && carriable.CanBeCarried(Ctx._carrier))
                {
                    Ctx._carrier.Pickup(carriable);
                    Ctx.ChangeState(Ctx.States[typeof(CarryingState)]);
                }

                if (detectable is IInteractable interactable && interactable.CanBeInteractedBy(Ctx._interactor))
                {
                    Ctx._interactor.InteractWith(interactable);
                }
            }
        }

        public void Enter()
        {
            
        }

        public void Exit()
        {
            
        }

        public void PhysicsUpdate(double delta) {}
    }
}