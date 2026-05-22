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
            if (Input.IsActionJustReleased("throw"))
            {
                Ctx._thrower.Throw();
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