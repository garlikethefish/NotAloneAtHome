namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class WalkingState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public WalkingState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Update(double delta)
        {
            if (Input.IsActionPressed("sprint"))
            {
                Ctx.ChangeState(Ctx.States[typeof(SprintingState)]);
            }

            if (Ctx._moveDirection == Vector2.Zero)
            {
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
            }

            if (!Ctx._footstepSound.Playing && !Ctx._waitBeforeWalkingSound)
                Ctx.PlayFootstepSound();
        }

        public void Enter()
        {
            Ctx.CanInteract = true;
            Ctx.CanSprint = true;
            Ctx.CanToggleMask = false;
        }

        public void Exit()
        {
            Ctx.CanInteract = false;
        }

        public void PhysicsUpdate(double delta)
        {
            
        }
    }
}