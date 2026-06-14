using Godot;
using NotAloneAtHome.state_machines.interfaces;

public class BanditShootState : IState
{
    private readonly Bandit _b;
    private bool _hasShot;
    private float _aimTime;
    private float _postKillTimer; // Timer for the cinematic pause

    public BanditShootState(Bandit bandit)
    {
        _b = bandit;
    }

    public void Enter()
    {
        _hasShot = false;
        _postKillTimer = 0f;
        _aimTime = 0.3f; 

        _b.SetNavTarget(_b.GlobalPosition);
        _b.Velocity = Vector2.Zero;
        _b.Anim?.Play("shoot_anim");
    }

    public void Exit() 
    {
        _postKillTimer = 0f;
    }

    public void Update(double delta) {}

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;
        
        _b.SetNavTarget(_b.GlobalPosition);
        _b.Velocity = Vector2.Zero;

        // 1. CINEMATIC PAUSE: If we successfully killed the player, stay put for 2 seconds
        if (_hasShot && _b.Player.IsDead)
        {
            _postKillTimer += dt;
            if (_postKillTimer >= 2.0f) // Adjust this for longer/shorter "victory" pose
            {
                _b.ChangeState(_b.States[typeof(BanditRoamState)]);
            }
            return;
        }

        // 2. INTERRUPT: If player died by OTHER means (not us), stop shooting immediately
        if (_b.Player == null || _b.Player.IsDead)
        {
            _b.ChangeState(_b.States[typeof(BanditRoamState)]);
            return;
        }

        // Tick down cooldowns
        if (_b.ShootTimer > 0f)
            _b.ShootTimer -= dt;

        // 3. INTERRUPT: If player broke line of sight
        if (!_b.CanSeePlayer())
        {
            _b.ChangeState(_b.States[typeof(BanditInvestigateState)]);
            return;
        }

        // 4. Handle Aiming -> Shooting sequence
        if (!_hasShot)
        {
            _aimTime -= dt;
            if (_aimTime <= 0f)
            {
                ExecuteAttack();
            }
        }
        else if (_b.ShootTimer <= 0f)
        {
            // If we are here, we missed or the player is still alive but we are reloading
            float distance = _b.GlobalPosition.DistanceTo(_b.Player.GlobalPosition);
            
            if (distance <= _b.ShootDistance)
            {
                _hasShot = false;
                _aimTime = 0.3f; 
                _b.Anim?.Play("shoot_anim");
            }
            else
            {
                _b.ChangeState(_b.States[typeof(BanditChaseState)]);
            }
        }
    }

    private void ExecuteAttack()
    {
        if (_hasShot || _b.ShootTimer > 0f)
            return;

        float distance = _b.GlobalPosition.DistanceTo(_b.Player.GlobalPosition);
        if (distance > _b.ShootDistance)
        {
            _b.ChangeState(_b.States[typeof(BanditChaseState)]);
            return;
        }

        _hasShot = true;
        _b.ShootTimer = _b.ShootCooldown;
        
        _b.Gunshot?.Play();
        _b.Player.Die();
        
        // No immediate state change here! 
        // PhysicsUpdate will catch the IsDead flag next frame and trigger the cinematic pause.
    }
}