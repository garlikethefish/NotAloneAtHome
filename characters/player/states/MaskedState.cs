namespace NotAloneAtHome.Characters.Player;

using Godot;
using NotAloneAtHome.state_machines.interfaces;

public partial class Player
{
    public class MaskedState : IState<Player>
    {
        public Player Ctx { get; private set; }

        public MaskedState(Player ctx)
        {
            Ctx = ctx;
        }

        public void Enter()
        {
            Ctx.isWearingMask = true;
            Ctx.CanSprint = false;
            Ctx.CanToggleMask = true;

            Ctx._currentSpeed = Ctx.NormalSpeed * Ctx.MaskSpeedMultiplier;
            Ctx._targetVisionRadius = Ctx.defaultMaskedVisionRadiuss;

            if (!Ctx._maskSound.Playing)
                Ctx._maskSound.Play();

            if (!Ctx._breathingParticles.Emitting)
                Ctx._breathingParticles.Emitting = true;
        }

        public void Update(double delta)
        {
            if (Input.IsActionJustPressed("toggle_mask"))
            {
                Ctx.ChangeState(Ctx.States[typeof(IdleState)]);
            }
        }

        public void PhysicsUpdate(double delta)
        {
        }

        public void Exit()
        {
            Ctx.isWearingMask = false;
            Ctx.CanSprint = true;

            Ctx._currentSpeed = Ctx.NormalSpeed;
            Ctx._targetVisionRadius = Ctx.defaultUnmaskedVisionRadiuss;

            if (Ctx._maskSound.Playing)
                Ctx._maskSound.Stop();

            Ctx._breathingParticles.Emitting = false;
        }
    }
}