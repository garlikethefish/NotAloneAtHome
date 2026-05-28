namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class AimingState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public AimingState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Update(double delta)
        {
            Ctx.SetFacingDirection(Ctx.GetGlobalMousePosition() - Ctx.GlobalPosition);

            if (Input.IsActionJustReleased("throw") && Ctx.Carriable is IThrowable throwable)
            {
                Ctx.Throw(throwable);
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
            }
        }

        public void Enter()
        {
            Ctx.IsAiming = true;
        }

        public void Exit()
        {
            Ctx.IsAiming = false;
        }

        public void PhysicsUpdate(double delta) {}
    }
}