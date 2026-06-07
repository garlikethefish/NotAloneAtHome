namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class SprintingState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public SprintingState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Update(double delta)
        {
            if (Ctx._moveDirection == Vector2.Zero)
            {
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
            }

            if (Input.IsActionJustReleased("sprint"))
            {
                Ctx.ChangeState(Ctx.States[typeof(WalkingState)]);
            }

            if (!Ctx._footstepSound.Playing && !Ctx._waitBeforeWalkingSound)
                Ctx.PlayFootstepSound();
        }

        public void Enter()
        {
            Ctx.CanInteract = true;
            Ctx._sprinting = true;
            Ctx._currentSpeed = Ctx.NormalSpeed * Ctx.SprintMultiplier;
        }

        public void Exit()
        {
            Ctx.CanInteract = false;
            Ctx._sprinting = false;
            Ctx._currentSpeed = Ctx.NormalSpeed;
        }

        public void PhysicsUpdate(double delta)
        {
            
        }
    }
}