using Godot;
using NotAloneAtHome.state_machines.interfaces;
using NotAloneAtHome.Scripts.Globals;

public class BanditChaseState : IState
{
    private Bandit _b;

    public BanditChaseState(Bandit bandit)
    {
        _b = bandit;
    }

    public void Enter()
    {
        _b.SetGlobalAlert(true);
    }

    public void Exit()
    {
        _b.SetGlobalAlert(false);
    }
    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        
        if (_b.Player == null || _b.Player.IsDead)
        {
            _b.SetGlobalAlert(false);
            _b.ChangeState(_b.States[typeof(BanditRoamState)]);
            return;
        }
        GameManager.Instance.AddSuspicion(18f * (float)delta);

        // ----------------------------
        // PRIORITY 1: direct vision chase
        // ----------------------------
        if (_b.CanSeePlayer())
        {
            _b.SetNavTarget(_b.Player.GlobalPosition);
        }
        else
        {
            // ----------------------------
            // PRIORITY 2: memory / last known position
            // ----------------------------
            Vector2? memory = _b.GetMemoryTarget();

            if (memory.HasValue)
            {
                _b.SetNavTarget(memory.Value);
            }
            else
            {
                // ----------------------------
                // PRIORITY 3: fallback roam
                // ----------------------------
                _b.ChangeState(_b.States[typeof(BanditInvestigateState)]);
                return;
            }
        }

        // ----------------------------
        // SHOOT LOGIC
        // ----------------------------
        float distance = _b.GlobalPosition.DistanceTo(_b.Player.GlobalPosition);

        if (distance <= _b.ShootDistance && _b.CanSeePlayer())
        {
            _b.ChangeState(_b.States[typeof(BanditShootState)]);
            return;
        }
    }
}