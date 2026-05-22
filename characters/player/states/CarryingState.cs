namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class CarryingState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public CarryingState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Update(double delta)
        {
            if (Input.IsActionPressed("throw"))
            {
                Ctx._thrower.StartAiming();
                Ctx.ChangeState(Ctx.States[typeof(AimingState)]);
            }

            if (Input.IsActionJustPressed("drop"))
            {
                Ctx._carrier.Drop();
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
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