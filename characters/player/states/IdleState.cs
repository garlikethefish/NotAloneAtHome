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
            Ctx.CanInteract = true;
            Ctx.CanSprint = true;
            Ctx.CanToggleMask = true;
            Ctx.ImmobileAnimation();
        }

        public void Exit()
        {
            Ctx.CanInteract = false;
        }

        public void PhysicsUpdate(double delta) {}
    }
}