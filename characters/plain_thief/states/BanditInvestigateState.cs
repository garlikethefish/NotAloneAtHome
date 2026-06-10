using Godot;
using NotAloneAtHome.state_machines.interfaces;

public class BanditInvestigateState : IState
{
    private Bandit _b;

    private float _searchTimer;
    private Vector2 _investigateTarget;

    public BanditInvestigateState(Bandit bandit) => _b = bandit;

    public void Enter()
    {
        _b.IsInvestigating = true;
        _b.SetGlobalAlert(true);

        _searchTimer = (float)GD.RandRange(1.0f, 2.5f);

        // PRIORITY: memory system (last seen or heard)
        var memory = _b.GetMemoryTarget();

        if (memory.HasValue)
        {
            _investigateTarget = memory.Value;
        }
        else if (_b.Player != null)
        {
            _investigateTarget = _b.Player.GlobalPosition;
        }
        else
        {
            _investigateTarget = _b.GlobalPosition;
        }

        _b.SetNavTarget(_investigateTarget);
    }

    public void Exit()
    {
        _b.IsInvestigating = false;
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;

        // -----------------------------
        // 1. IMMEDIATE VISION OVERRIDE
        // -----------------------------
        if (_b.CanSeePlayer())
        {
            _b.ChangeState(_b.States[typeof(BanditChaseState)]);
            return;
        }

        
        // -----------------------------
        // 3. ARRIVED AT TARGET
        // -----------------------------
        if (_b.Nav.IsNavigationFinished())
        {
            _searchTimer -= dt;

            if (_searchTimer <= 0f)
            {
                // expand search radius gradually
                Vector2 randomOffset = new Vector2(
                    (float)GD.RandRange(-120, 120),
                    (float)GD.RandRange(-120, 120)
                );

                _investigateTarget += randomOffset;
                _b.SetNavTarget(_investigateTarget);

                _searchTimer = (float)GD.RandRange(1.0, 2.5);
            }

            // give up after long search
            if (_b.GetMemoryTarget() == null)
            {
                _b.ChangeState(_b.States[typeof(BanditRoamState)]);
                return;
            }
        }
    }
}