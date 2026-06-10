using Godot;
using NotAloneAtHome.state_machines.interfaces;

public class BanditShootState : IState
{
    private Bandit _b;

    private bool _hasShot = false;

    public BanditShootState(Bandit bandit)
    {
        _b = bandit;
    }

    public void Enter()
    {
        _hasShot = false;

        // Stop movement completely
        _b.SetNavTarget(_b.GlobalPosition);
        _b.Velocity = Vector2.Zero;

        // Play shoot animation once (will be preserved because Bandit.cs skips animation override in this state if you applied the fix)
        _b.Anim?.Play("shoot_anim");

        ExecuteAttack();
    }

    public void Exit()
    {
    }

    public void Update(double delta)
    {
    }

    public void PhysicsUpdate(double delta)
    {
        // Keep position locked
        _b.SetNavTarget(_b.GlobalPosition);

        // Wait for cooldown before switching states
        if (_b.ShootTimer > 0)
            return;

        if (_b.Player == null || _b.Player.IsDead)
        {
            _b.ChangeState(_b.States[typeof(BanditRoamState)]);
            return;
        }

        // Decide next state after shooting cycle
        if (_b.CanSeePlayer())
        {
            _b.ChangeState(_b.States[typeof(BanditChaseState)]);
        }
        else
        {
            _b.ChangeState(_b.States[typeof(BanditInvestigateState)]);
        }
    }

    private void ExecuteAttack()
    {
        if (_hasShot)
            return;

        if (_b.Player == null)
            return;

        if (_b.ShootTimer > 0)
            return;

        _hasShot = true;

        _b.ShootTimer = _b.ShootCooldown;

        _b.Gunshot?.Play();

        _b.Player.Die();
    }
}