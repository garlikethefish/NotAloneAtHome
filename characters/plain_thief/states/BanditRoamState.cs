using Godot;
using NotAloneAtHome.state_machines.interfaces;

public class BanditRoamState : IState
{
    private Bandit _b;

    private float _wanderTimer;

    public BanditRoamState(Bandit bandit) => _b = bandit;

    public void Enter()
    {
        _wanderTimer = 0f;
        SetNewRandomTarget();
    }

    public void Exit() { }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        // -----------------------------
        // 1. DIRECT VISION OVERRIDE
        // -----------------------------
        if (_b.CanSeePlayer())
        {
            _b.SetGlobalAlert(true);
            _b.ChangeState(_b.States[typeof(BanditChaseState)]);
            return;
        }

        // -----------------------------
        // 2. MEMORY OVERRIDE (IMPORTANT)
        // -----------------------------
        var memory = _b.GetMemoryTarget();
        if (memory.HasValue)
        {
            _b.SetNavTarget(memory.Value);

            // if close enough, switch to investigate/chase
            if (_b.GlobalPosition.DistanceTo(memory.Value) < 80f)
            {
                _b.ChangeState(_b.States[typeof(BanditInvestigateState)]);
                return;
            }

            return;
        }

        // -----------------------------
        // 3. WANDER BEHAVIOR
        // -----------------------------
        _wanderTimer -= (float)delta;

        if (_wanderTimer <= 0f || _b.Nav.IsNavigationFinished())
        {
            SetNewRandomTarget();
        }
    }

private void SetNewRandomTarget()
{
    Vector2 randomPos = new Vector2(
        (float)GD.RandRange(-200, 900),
        (float)GD.RandRange(-200, 700)
    );

    _b.SetNavTarget(randomPos);

    _wanderTimer = (float)GD.RandRange(2.0, 5.0);
}
}