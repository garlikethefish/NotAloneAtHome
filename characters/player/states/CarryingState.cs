namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.Components;
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
            if (Input.IsActionPressed("throw") && Ctx.CarrierComponent.CarriableComp.ParentHas<ThrowableComponent>())
            {
                Ctx.StartAiming();
                Ctx.ChangeState(Ctx.States[typeof(AimingState)]);
            }

            if (Input.IsActionJustPressed("drop"))
            {
                Ctx.Drop();
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
            }
        }

        public void Enter()
        {
            Ctx.CanInteract = true;
            Ctx.CanSprint = false;
            Ctx.CanToggleMask = false;
            Ctx._currentSpeed = Ctx.NormalSpeed * Ctx.CarrySpeedMultiplier;
        }

        public void Exit()
        {
            Ctx.CanInteract = false;
            Ctx._currentSpeed = Ctx.NormalSpeed;
        }

        public void PhysicsUpdate(double delta) {}
    }
}