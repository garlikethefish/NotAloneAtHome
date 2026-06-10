using Godot;
using NotAloneAtHome.state_machines.interfaces;

public class BanditAlertState : IState
{
    private Bandit _b;

    private float _searchTimer = 0f;
    private const float SearchDuration = 4f;

    public BanditAlertState(Bandit bandit)
    {
        _b = bandit;
    }

    public void Enter()
    {
        _b.SetGlobalAlert(true);

        // no CurrentSpeed usage anymore
        _searchTimer = SearchDuration;

        Vector2? memory = _b.GetMemoryTarget();
        if (memory.HasValue)
        {
            _b.SetNavTarget(memory.Value);
        }
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
            _b.ChangeState(_b.States[typeof(BanditRoamState)]);
            return;
        }

        float dt = (float)delta;
        _searchTimer -= dt;

        // PRIORITY 1: direct vision
        if (_b.CanSeePlayer())
        {
            _b.SetNavTarget(_b.Player.GlobalPosition);
            _b.ChangeState(_b.States[typeof(BanditChaseState)]);
            return;
        }

        // PRIORITY 2: memory tracking
        Vector2? memory = _b.GetMemoryTarget();

        if (memory.HasValue)
        {
            _b.SetNavTarget(memory.Value);
        }
        else
        {
            // fallback: search sweep
            Vector2 randomOffset = new Vector2(
                (float)GD.RandRange(-120, 120),
                (float)GD.RandRange(-120, 120)
            );

            _b.SetNavTarget(_b.GlobalPosition + randomOffset);
        }

        // EXIT
        if (_searchTimer <= 0f)
        {
            if (memory.HasValue)
                _b.ChangeState(_b.States[typeof(BanditInvestigateState)]);
            else
                _b.ChangeState(_b.States[typeof(BanditRoamState)]);
        }
    }
}