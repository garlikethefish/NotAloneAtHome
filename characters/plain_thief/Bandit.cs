using Godot;
using System;
using System.Collections.Generic;
using NotAloneAtHome.state_machines.interfaces;
using NotAloneAtHome.Characters.Player;

public partial class Bandit : CharacterBody2D, IStateMachine
{
    [ExportGroup("Movement")]
    [Export] public float Speed = 75f;
    [Export] public float ChaseSpeed = 110f;
    [Export] public float Acceleration = 6f;
    [Export] public float Deceleration = 8f;
    [Export] public float TurnSmoothing = 10f;

    [ExportGroup("Vision")]
    [Export] public float VisionRange = 220f;
    [Export] public float VisionAngle = 70f;
    [Export] public float VisionConeOffset = 12f;

    [ExportGroup("Combat")]
    [Export] public float ShootCooldown = 1.2f;
    [Export] public float ShootDistance = 200f;

    [ExportGroup("Hearing")]
    [Export] public float HearingRange = 260f;
    [Export] public float SprintNoiseMultiplier = 1.8f;

    [ExportGroup("Debug")]
    [Export] public bool DebugVision = false;

    public Dictionary<Type, IState> States { get; set; } = new();
    public IState CurrentState { get; private set; }

    public Player Player { get; private set; }
    public NavigationAgent2D Nav { get; private set; }
    public RayCast2D SightRay { get; private set; }
    public AnimatedSprite2D Anim { get; private set; }
    public AudioStreamPlayer2D Gunshot { get; private set; }

    // Vision cone node (MUST be child of bandit)
    private Node2D _visionCone;
    private ShaderMaterial _visionMat;

    public float ShootTimer { get; set; }
    public bool IsGlobalAlert { get; private set; }

    // REQUIRED for investigate state
    public bool IsInvestigating { get; set; }

    // movement
    public Vector2 Forward { get; private set; } = Vector2.Down;
    private Vector2 _smoothedForward = Vector2.Down;
    private Vector2 _velocity = Vector2.Zero;

    // memory
    private Vector2? _lastSeenPosition;
    private Vector2? _lastHeardPosition;
    private float _memoryTimer;
    private const float MemoryDuration = 6f;

    // vision FX
    private float _pulseTime;
    private float _scanTime;

    [Signal] public delegate void NewStateEventHandler(string stateName);

    public override void _Ready()
    {
        Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Nav = GetNode<NavigationAgent2D>("NavigationAgent2D");
        SightRay = GetNode<RayCast2D>("SightRay");
        Gunshot = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        Player = GetTree().GetFirstNodeInGroup("player") as Player;

        _visionCone = GetNodeOrNull<Node2D>("VisionCone");
        if (_visionCone != null)
            _visionMat = _visionCone.GetNodeOrNull<ColorRect>("ColorRect")?.Material as ShaderMaterial;

        SightRay.AddException(this);

        States[typeof(BanditRoamState)] = new BanditRoamState(this);
        States[typeof(BanditChaseState)] = new BanditChaseState(this);
        States[typeof(BanditInvestigateState)] = new BanditInvestigateState(this);
        States[typeof(BanditShootState)] = new BanditShootState(this);
        States[typeof(BanditAlertState)] = new BanditAlertState(this);

        ChangeState(States[typeof(BanditRoamState)]);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        CurrentState?.PhysicsUpdate(delta);

        UpdatePerception(dt);
        UpdateMovement(dt);
        MoveAndSlide();

        UpdateForward(dt);
        UpdateMemory(dt);
        UpdateAnimation();
        UpdateVisionCone(dt);
    }

    // ---------------- MOVEMENT ----------------

    private void UpdateMovement(float dt)
    {
        Vector2 targetVelocity = Vector2.Zero;

        if (!Nav.IsNavigationFinished())
        {
            Vector2 next = Nav.GetNextPathPosition();
            Vector2 dir = GlobalPosition.DirectionTo(next);

            float speed = IsGlobalAlert ? ChaseSpeed : Speed;
            targetVelocity = dir * speed;
        }

        float accel = targetVelocity.Length() > _velocity.Length()
            ? Acceleration
            : Deceleration;

        _velocity = _velocity.Lerp(targetVelocity, accel * dt);
        Velocity = _velocity;
    }

    private void UpdateForward(float dt)
    {
        Vector2 target = _velocity.Normalized();

        float t = 1f - Mathf.Exp(-TurnSmoothing * dt);
        if (!Nav.IsNavigationFinished())
        {
            _smoothedForward = _smoothedForward.Lerp(target, t).Normalized();
            Vector2 next =
                GlobalPosition.DirectionTo(
                    Nav.GetNextPathPosition()
                );

            Forward = Forward.Lerp(next, t).Normalized();
        }
        else if (_velocity.LengthSquared() > 0.001f)
        {
            Forward = Forward.Lerp(
                _velocity.Normalized(),
                t
            ).Normalized();
        }
    }

    // ---------------- VISION CONE (STABLE + NO PLAYER LOCK) ----------------

    private void UpdateVisionCone(float dt)
    {
        if (_visionCone == null)
            return;

        _pulseTime += dt;

        float angle = Forward.Angle();

        // ALWAYS attached to bandit (never player drift)
        _visionCone.GlobalPosition = GlobalPosition + Forward * VisionConeOffset;
        _visionCone.Rotation = angle;

        // SCAN SWEEP WHEN SEARCHING
        if (IsGlobalAlert && !CanSeePlayer())
        {
            _scanTime += dt;
        }
        else
        {
            _scanTime = 0f;
        }

        float scanOffset = Mathf.Sin(_scanTime * 2.0f) * 0.15f;

        float dynamicAngle = VisionAngle;

        if (IsGlobalAlert)
            dynamicAngle *= 0.75f; // narrowing cone when alert

        if (_visionMat != null)
        {
            _visionCone.GlobalPosition = GlobalPosition;
            _visionCone.GlobalRotation = Forward.Angle();

            if (_visionMat != null)
            {
                _visionMat.SetShaderParameter("forward", Vector2.Right);
            }
            _visionMat.SetShaderParameter("vision_angle", dynamicAngle);
            _visionMat.SetShaderParameter("vision_range", VisionRange);

            _visionMat.SetShaderParameter("pulse", _pulseTime);
            _visionMat.SetShaderParameter("scan_offset", scanOffset);

            _visionMat.SetShaderParameter("alert_strength", IsGlobalAlert ? 1f : 0f);
            _visionMat.SetShaderParameter("focus", CanSeePlayer() ? 1f : 0f);
        }
    }

    // ---------------- PERCEPTION ----------------

    private void UpdatePerception(float dt)
    {
        if (Player == null || Player.IsDead)
            return;

        if (Player.isWearingMask)
            return;

        Vector2 toPlayer = Player.GlobalPosition - GlobalPosition;
        float dist = toPlayer.Length();

        float speed = Player.Velocity.Length();

        if (speed > 60f)
        {
            float hearing = speed / 120f;

            if (dist <= HearingRange * hearing * SprintNoiseMultiplier)
            {
                _lastHeardPosition = Player.GlobalPosition;
                _memoryTimer = MemoryDuration;
            }
        }

        if (dist <= VisionRange)
        {
            Vector2 dir = toPlayer.Normalized();

            float angle = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(Forward.Dot(dir), -1f, 1f)));

            if (angle <= VisionAngle * 0.5f)
            {
                SightRay.TargetPosition = toPlayer;
                SightRay.ForceRaycastUpdate();

                if (!SightRay.IsColliding() || SightRay.GetCollider() == Player)
                {
                    _lastSeenPosition = Player.GlobalPosition;
                    _memoryTimer = MemoryDuration;
                }
            }
        }
    }

    private void UpdateMemory(float dt)
    {
        if (_memoryTimer > 0)
            _memoryTimer -= dt;
        else
        {
            _lastSeenPosition = null;
            _lastHeardPosition = null;

            // start scan when losing target
            if (IsGlobalAlert)
                _scanTime = 0f;
        }
    }

    public Vector2? GetMemoryTarget()
    {
        if (_lastSeenPosition.HasValue)
            return _lastSeenPosition;

        if (_lastHeardPosition.HasValue)
            return _lastHeardPosition;

        return null;
    }

    // ---------------- VISION CHECK ----------------

    public bool CanSeePlayer()
    {
        if (Player == null || Player.IsDead)
            return false;

        if (Player.isWearingMask)
            return false;

        Vector2 toPlayer = Player.GlobalPosition - GlobalPosition;

        if (toPlayer.Length() > VisionRange)
            return false;

        Vector2 dir = toPlayer.Normalized();

        float angle = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(Forward.Dot(dir), -1f, 1f)));

        if (angle > VisionAngle * 0.5f)
            return false;

        SightRay.TargetPosition = toPlayer;
        SightRay.ForceRaycastUpdate();

        return !SightRay.IsColliding() || SightRay.GetCollider() == Player;
    }

    // ---------------- ANIMATION ----------------

    private string _lastFacing = "down";

    private void UpdateAnimation()
    {
        if (Anim == null)
            return;

        // Don't override shoot animation
        if (CurrentState == States[typeof(BanditShootState)])
            return;

        Vector2 v = Velocity;

        if (Mathf.Abs(v.X) > Mathf.Abs(v.Y))
        {
            _lastFacing = "side";
            Anim.FlipH = v.X < 0;
        }
        else if (v.Y < 0)
        {
            _lastFacing = "up";
        }
        else if (v.Y > 0)
        {
            _lastFacing = "down";
        }
        string anim =
            v.LengthSquared() > 0.01f
            ? $"walk_{_lastFacing}"
            : $"idle_{_lastFacing}";

        if (Anim.Animation != anim)
            Anim.Play(anim);
    }

    // ---------------- STATE ----------------

    public void SetNavTarget(Vector2 pos) => Nav.TargetPosition = pos;
    public void SetGlobalAlert(bool value) => IsGlobalAlert = value;

    public void ChangeState(IState next)
    {
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState?.Enter();

        EmitSignal(SignalName.NewState, CurrentState?.GetType().Name ?? "None");
    }

    public void Update(double delta) => CurrentState?.Update(delta);
    public void PhysicsUpdate(double delta) => CurrentState?.PhysicsUpdate(delta);
}