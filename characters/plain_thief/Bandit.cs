using Godot;
using NotAloneAtHome.Characters.Player;

public partial class Bandit : CharacterBody2D
{
    [Export] public float Speed           = 120.0f;
    [Export] public float ChaseSpeed      = 180.0f;
    [Export] public float VisionAngle     = 70.0f;
    [Export] public float VisionRange     = 220.0f;
    [Export] public float ShootDistance   = 200.0f;
    [Export] public float ShootCooldown   = 1.2f;
    [Export] public float RoamWaitTime    = 1.5f;
    [Export] public float HearingRadius   = 160.0f;
    [Export] public float InvestigateTime = 2.0f;
    [Export] public float MinX            = -100.0f;
    [Export] public float MaxX            = 800.0f;
    [Export] public float MinY            = -100.0f;
    [Export] public float MaxY            = 600.0f;

    private Player             _player;
    private NavigationAgent2D  _navAgent;
    private AnimatedSprite2D   _anim;
    private RayCast2D          _sightRay;
    private AudioStreamPlayer2D _gunshotSound;
    private AudioStreamPlayer2D _footstepSound;

    private float   _roamTimer        = 0f;
    private bool    _hasTarget        = false;
    private float   _shootTimer       = 0f;
    private float   _investigateTimer = 0f;
    private Vector2 _investigateTarget = Vector2.Zero;
    private bool    _isInvestigating  = false;
    private string  _lastFacing       = "down";
    private bool    _isShooting       = false;
    private bool    _playerEliminated = false;
    private Vector2 _lookDirection    = Vector2.Right;
    private bool    _isGlobalAlert    = false;
    private bool    _wait             = false;
    private bool    _sprinting        = false;

    public override void _Ready()
    {
        _anim          = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sightRay      = GetNode<RayCast2D>("SightRay");
        _gunshotSound  = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        _footstepSound = GetNode<AudioStreamPlayer2D>("FootstepSound");
        _navAgent      = GetNode<NavigationAgent2D>("NavigationAgent2D");

        // GameManager.OnMaxSuspicion.Connect(Callable.From(_OnGlobalAlert));
        // GameManager.OnMaxItemsStolen.Connect(Callable.From(_OnGlobalAlert));

        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        AddToGroup("bandits");
        UpdateAnimation();
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;
        _shootTimer -= d;

        if (_playerEliminated)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            UpdateAnimation();
            return;
        }

        if (_isGlobalAlert && _player != null && !_player.IsDead)
            ChasePlayerGlobally();
        // else if (_player != null && CanSeePlayer() && !_player.MaskOn)
        // {
        //     _isInvestigating = false;
        //     ChaseAndAttack();
        // }
        // else if (_player != null && CanHearPlayer() && !_player.MaskOn)
        //     StartInvestigating(_player.GlobalPosition);
        else if (_isInvestigating)
            Investigate(d);
        else
            Roam(d);

        if (Velocity.Length() > 5)
            _lookDirection = Velocity.Normalized();

        Velocity = _navAgent.GetVelocity();
        MoveAndSlide();
        UpdateAnimation();
    }

    private void _OnGlobalAlert()
    {
        _isGlobalAlert   = true;
        _isInvestigating = false;
        _hasTarget       = false;
    }

    private void ChasePlayerGlobally()
    {
        _navAgent.TargetPosition = _player.GlobalPosition;
        if (_navAgent.IsNavigationFinished()) return;

        var nextPos        = _navAgent.GetNextPathPosition();
        var dir            = GlobalPosition.DirectionTo(nextPos);
        var desiredVelocity = dir * ChaseSpeed * 1.3f;
        Velocity = Velocity.MoveToward(desiredVelocity, 1100f * (float)GetPhysicsProcessDeltaTime());
        _navAgent.SetVelocity(Velocity);
        Velocity = _navAgent.GetVelocity();
        TryShootPlayer();
    }

    private void Roam(float delta)
    {
        if (!_hasTarget)
        {
            _roamTimer -= delta;
            if (_roamTimer <= 0)
            {
                SetNewRoamTarget();
                _roamTimer = RoamWaitTime;
            }
            Velocity = Vector2.Zero;
            return;
        }

        if (_navAgent.IsNavigationFinished())
        {
            _hasTarget = false;
            Velocity   = Vector2.Zero;
            return;
        }

        if (!_footstepSound.Playing && !_wait)
            PlayFootstepSound();

        var nextPos         = _navAgent.GetNextPathPosition();
        var dir             = GlobalPosition.DirectionTo(nextPos);
        var desiredVelocity = dir * Speed;
        Velocity = Velocity.MoveToward(desiredVelocity, 800f * delta);
        _navAgent.SetVelocity(Velocity);
        Velocity = _navAgent.GetVelocity();
    }

    private void StartInvestigating(Vector2 pos)
    {
        _investigateTarget       = pos;
        _navAgent.TargetPosition = pos;
        _isInvestigating         = true;
        _investigateTimer        = InvestigateTime;
    }

    private void Investigate(float delta)
    {
        float distToTarget = GlobalPosition.DistanceTo(_investigateTarget);
        if (distToTarget < 14f)
        {
            Velocity = Vector2.Zero;
            _navAgent.SetVelocity(Vector2.Zero);
            _navAgent.TargetPosition = GlobalPosition;
            _investigateTimer -= delta;
            if (_investigateTimer <= 0)
                _isInvestigating = false;
            return;
        }

        var nextPos         = _navAgent.GetNextPathPosition();
        var dir             = GlobalPosition.DirectionTo(nextPos);
        var desiredVelocity = dir * Speed * 0.7f;
        Velocity = Velocity.MoveToward(desiredVelocity, 600f * delta);
        _navAgent.SetVelocity(Velocity);
        Velocity = _navAgent.GetVelocity();
    }

    private bool CanHearPlayer()
    {
        if (_player == null) return false;
        if (_player.Velocity.Length() < 160f) return false;
        return GlobalPosition.DistanceTo(_player.GlobalPosition) <= HearingRadius;
    }

    private void SetNewRoamTarget()
    {
        var randomPoint = new Vector2(
            (float)GD.RandRange(MinX, MaxX),
            (float)GD.RandRange(MinY, MaxY)
        );
        _navAgent.TargetPosition = randomPoint;
        _hasTarget               = true;
    }

    private void ChaseAndAttack()
    {
        _navAgent.TargetPosition = _player.GlobalPosition;
        if (_navAgent.IsNavigationFinished()) return;

        var nextPos         = _navAgent.GetNextPathPosition();
        var dir             = GlobalPosition.DirectionTo(nextPos);
        var desiredVelocity = dir * ChaseSpeed;
        Velocity = Velocity.MoveToward(desiredVelocity, 900f * (float)GetPhysicsProcessDeltaTime());
        _navAgent.SetVelocity(Velocity);
        Velocity = _navAgent.GetVelocity();
        TryShootPlayer();
    }

    private bool HasLineOfSightToPlayer()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query      = PhysicsRayQueryParameters2D.Create(GlobalPosition, _player.GlobalPosition);
        query.Exclude          = [GetRid()];
        query.CollideWithAreas  = false;
        query.CollideWithBodies = true;
        var result = spaceState.IntersectRay(query);
        return !(result.Count > 0 && result["collider"].As<Node>() != _player);
    }

    private void TryShootPlayer()
    {
        if (_shootTimer > 0) return;
        if (!HasLineOfSightToPlayer()) return;
        if (GlobalPosition.DistanceTo(_player.GlobalPosition) > ShootDistance) return;

        _shootTimer      = ShootCooldown;
        _playerEliminated = true;
        _isShooting      = true;
        _anim.Play("shoot_anim");
        _gunshotSound?.Play();
        _player.Die();
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;
        var toPlayer = _player.GlobalPosition - GlobalPosition;
        if (toPlayer.Length() > VisionRange) return false;

        float angleToPlayer = Mathf.RadToDeg(_lookDirection.AngleTo(toPlayer.Normalized()));
        if (Mathf.Abs(angleToPlayer) > VisionAngle * 0.5f) return false;

        _sightRay.TargetPosition = toPlayer;
        _sightRay.ForceRaycastUpdate();
        if (_sightRay.IsColliding() && _sightRay.GetCollider() != _player) return false;

        return true;
    }

    public override void _Draw()
    {
        var coneColor = new Color(1, 0, 0, 0.15f);
        float halfAngle = Mathf.DegToRad(VisionAngle * 0.5f);
        int rays = 20;
        var points = new Vector2[rays + 2];
        points[0] = Vector2.Zero;
        for (int i = 0; i <= rays; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / rays);
            var dir     = _lookDirection.Rotated(angle);
            points[i + 1] = CastVisionRay(dir);
        }
        DrawPolygon(points, [coneColor]);
    }

    private async void PlayFootstepSound()
    {
        _footstepSound.Play();
        _wait = true;
        await ToSignal(GetTree().CreateTimer(_sprinting ? 0.3f : 0.4f), SceneTreeTimer.SignalName.Timeout);
        _wait = false;
    }

    private Vector2 CastVisionRay(Vector2 direction)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var from       = GlobalPosition;
        var to         = from + direction * VisionRange;
        var query      = PhysicsRayQueryParameters2D.Create(from, to);
        query.Exclude          = [GetRid()];
        query.CollideWithAreas  = false;
        query.CollideWithBodies = true;
        var result = spaceState.IntersectRay(query);
        var hitPos = result.Count > 0 ? result["position"].As<Vector2>() : to;
        return ToLocal(hitPos);
    }

    private void UpdateAnimation()
    {
        if (_isShooting)
        {
            if (!_anim.IsPlaying())
            {
                _isShooting = false;
                _anim.Play("idle_" + _lastFacing);
            }
            return;
        }

        bool moving = Velocity.Length() > 10f;
        if (moving)
        {
            if (Velocity.Y < 0)      _lastFacing = "up";
            else if (Velocity.Y > 0) _lastFacing = "down";
            else                     _lastFacing = "side";
        }

        _anim.Play(moving ? "walk_" + _lastFacing : "idle_" + _lastFacing);
        _anim.FlipH = Velocity.X < 0;
    }
}