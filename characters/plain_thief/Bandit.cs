using System;
using System.Collections.Generic;
using NotAloneAtHome.state_machines.interfaces;
using NotAloneAtHome.Characters.Player;
using NotAloneAtHome.Scripts.Globals;
using Godot;

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
    
    [ExportGroup("Suspicion")]
    [Export] public float RoamingSuspicionRate = 0.5f;
    [Export] public float SeenSuspicionRate = 6f;
    [Export] public float ChaseSuspicionRate = 12f;
    [Export] public float MaskReductionMultiplier = 0.4f;

    [ExportGroup("Debug")]
    [Export] public bool DebugVision = false;

    public Dictionary<Type, IState> States { get; set; } = new();
    public IState CurrentState { get; private set; }

    public Player Player { get; private set; }
    public NavigationAgent2D Nav { get; private set; }
    public RayCast2D SightRay { get; private set; }
    public AnimatedSprite2D Anim { get; private set; }
    public AudioStreamPlayer2D Gunshot { get; private set; }
    private NoiseReciever _noiseReceiver;
    private Area2D _visionArea;

    private readonly HashSet<Node2D> _visionTargets = [];

    private Node2D _visionCone;
    private ShaderMaterial _visionMat;
    private enum SuspicionMode
    {
        Roam,
        Alert,
        Investigate,
        Chase
    }

    private SuspicionMode _mode;

    public float ShootTimer { get; set; }
    public bool IsGlobalAlert { get; private set; }
    public bool IsInvestigating { get; set; }

    // Movement tracking
    public Vector2 Forward { get; private set; } = Vector2.Down;
    private Vector2 _smoothedForward = Vector2.Down;
    private Vector2 _velocity = Vector2.Zero;

    // Memory
    private Vector2? _lastSeenPosition;
    private Vector2? _lastHeardPosition;
    private float _memoryTimer;
    private const float MemoryDuration = 6f;

    // Vision FX
    private float _pulseTime;
    private float _scanTime;

    // --- NEW: Cat-like Stuck Detection variables ---
    private Vector2 _stuckCheckPos;
    private float _stuckTimer;
    private RandomNumberGenerator _rng = new();

    private void OnVisionEnter(Node2D body) => _visionTargets.Add(body);
    private void OnVisionExit(Node2D body) => _visionTargets.Remove(body);

    public override void _Ready()
    {
        Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Nav = GetNode<NavigationAgent2D>("NavigationAgent2D");
        SightRay = GetNode<RayCast2D>("SightRay");
        Gunshot = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        _noiseReceiver = GetNode<NoiseReciever>("NoiseReciever");
        _visionArea = GetNode<Area2D>("VisionArea");

        _visionArea.BodyEntered += OnVisionEnter;
        _visionArea.BodyExited += OnVisionExit;

        Player = GetTree().GetFirstNodeInGroup("player") as Player;

        _visionCone = GetNodeOrNull<Node2D>("VisionCone");
        if (_visionCone != null)
            _visionMat = _visionCone.GetNodeOrNull<ColorRect>("ColorRect")?.Material as ShaderMaterial;

        SightRay.AddException(this);

        // --- NEW: Configure Bandit Navigation to match the Cat's settings ---
        Nav.PathDesiredDistance = 6f;
        Nav.TargetDesiredDistance = 16f; 
        Nav.AvoidanceEnabled = true;
        
        // This stops the bandit from clipping wall corners or trying to squeeze 
        // through gaps smaller than its collision shape. Adjust slightly if the sprite is bulky.
        Nav.Radius = 16f; 
        Nav.MaxSpeed = ChaseSpeed * 1.5f;

        Nav.TimeHorizonAgents = 1.5f;
        Nav.TimeHorizonObstacles = 2.5f;
        Nav.NeighborDistance = 100f;
        Nav.MaxNeighbors = 12;

        _stuckCheckPos = GlobalPosition;

        // State setups
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

        // UpdatePerception(dt);
        UpdateSuspicion(dt);
        
        // Process core velocity calculations
        UpdateMovement(dt);
        CheckIfStuck(dt);

        // --- FIXED: Fetch the calculated safe avoidance velocity from the NavAgent ---
        Velocity = Nav.GetVelocity();
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

        // Apply internal acceleration smoothing
        _velocity = _velocity.Lerp(targetVelocity, accel * dt);
        
        // --- FIXED: Instead of pushing directly to the body, pass it to the Nav system 
        // so it recalculates paths around obstacles.
        Nav.Velocity = _velocity;
    }

    // ---------------- STUCK DETECTION ----------------

    private void CheckIfStuck(float dt)
    {
        // Don't care if we've already arrived at our destination
        if (Nav.IsNavigationFinished())
        {
            _stuckTimer = 0f;
            _stuckCheckPos = GlobalPosition;
            return;
        }

        _stuckTimer += dt;
        if (_stuckTimer < 0.8f) return;

        float moved = GlobalPosition.DistanceTo(_stuckCheckPos);

        // If the bandit has barely moved despite wanting to walk, force a recalibration
        if (moved < 6f)
        {
            Unstick();
        }

        _stuckCheckPos = GlobalPosition;
        _stuckTimer = 0f;
    }

    private void Unstick()
    {
        // For a Bandit, breaking state into a random wander might break a pursuit. 
        // Instead, we nudge the current target slightly or force-clear the current path 
        // segment to force the NavigationServer to re-route around whatever edge it's catching on.
        Vector2 currentTarget = Nav.TargetPosition;
        float nudgeAngle = _rng.RandfRange(0f, Mathf.Tau);
        Vector2 nudge = new Vector2(Mathf.Cos(nudgeAngle), Mathf.Sin(nudgeAngle)) * 15f;

        Nav.TargetPosition = currentTarget + nudge;
        Nav.Velocity = Vector2.Zero;
    }

    // ---------------- FORWARD & ROTATION ----------------

    private void UpdateForward(float dt)
    {
        Vector2 target = _velocity.Normalized();
        float t = 1f - Mathf.Exp(-TurnSmoothing * dt);
        
        if (!Nav.IsNavigationFinished())
        {
            _smoothedForward = _smoothedForward.Lerp(target, t).Normalized();
            Vector2 next = GlobalPosition.DirectionTo(Nav.GetNextPathPosition());
            Forward = Forward.Lerp(next, t).Normalized();
        }
        else if (_velocity.LengthSquared() > 0.001f)
        {
            Forward = Forward.Lerp(_velocity.Normalized(), t).Normalized();
        }
    }

    // ---------------- SUSPICION ----------------

    private void UpdateSuspicion(float dt)
    {
        if (GameManager.Instance == null || Player == null)
            return;

        UpdateSuspicionMode();

        float rate = _mode switch
        {
            SuspicionMode.Roam => RoamingSuspicionRate,
            SuspicionMode.Alert => SeenSuspicionRate,
            SuspicionMode.Investigate => SeenSuspicionRate * 1.5f,
            SuspicionMode.Chase => ChaseSuspicionRate,
            _ => RoamingSuspicionRate
        };

        if (Player.isWearingMask)
            rate *= MaskReductionMultiplier;

        GameManager.Instance.AddSuspicion(rate * dt);
    }

    // ---------------- VISION CONE ----------------

    private void UpdateVisionCone(float dt)
    {
        if (_visionCone == null)
            return;

        _pulseTime += dt;
        float angle = Forward.Angle();

        _visionCone.GlobalPosition = GlobalPosition + Forward * VisionConeOffset;
        _visionCone.Rotation = angle;

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
            dynamicAngle *= 0.75f;

        if (_visionMat != null)
        {
            _visionCone.GlobalRotation = Forward.Angle();
            _visionMat.SetShaderParameter("forward", Vector2.Right);
            _visionMat.SetShaderParameter("vision_angle", dynamicAngle);
            _visionMat.SetShaderParameter("vision_range", VisionRange);
            _visionMat.SetShaderParameter("pulse", _pulseTime);
            _visionMat.SetShaderParameter("scan_offset", scanOffset);
            _visionMat.SetShaderParameter("alert_strength", IsGlobalAlert ? 1f : 0f);
            _visionMat.SetShaderParameter("focus", CanSeePlayer() ? 1f : 0f);
        }
    }

    // // ---------------- PERCEPTION ----------------
    // private void UpdateAcousticPerception(float dt)
    // {
    //     if (_noiseReceiver == null || Player == null || Player.IsDead) 
    //         return;

    //     // Check if ambient/direct sound picked up by the Area2D crosses our threshold
    //     if (_noiseReceiver.CurrentNoise > 30f)
    //     {
    //         Vector2? acousticTarget = _noiseReceiver.GetLoudestNoisePosition();

    //         if (acousticTarget.HasValue)
    //         {
    //             // Lock onto where the sound wave was created without cheating coordinates
    //             _lastHeardPosition = acousticTarget.Value;
    //             _memoryTimer = MemoryDuration;
    //             SetGlobalAlert(true);

    //             // Interrupt normal roaming paths if not actively engaged in gunfights/direct chasing
    //             if (CurrentState != States[typeof(BanditChaseState)] && CurrentState != States[typeof(BanditShootState)])
    //             {
    //                 if (CurrentState != States[typeof(BanditInvestigateState)])
    //                 {
    //                     IsInvestigating = true;
                        
    //                     // Route pathfinding directly towards the noise node vector location
    //                     SetNavTarget(acousticTarget.Value);
                        
    //                     // Drop current state to process sound coordinates
    //                     ChangeState(States[typeof(BanditInvestigateState)]);
    //                 }
    //             }
    //         }
    //     }
    // }
    // ---------------- PERCEPTION ----------------

    // private void UpdatePerception(float dt)
    // {
    //     if (Player == null || Player.IsDead)
    //         return;

    //     // --- CAT-LIKE NOISE REACTION SYSTEM ---
    //     if (_noiseReceiver != null && _noiseReceiver.CurrentNoise > 30f) // Matches Cat threshold
    //     {
    //         Vector2? noiseSourcePos = _noiseReceiver.GetLoudestNoisePosition();
            
    //         if (noiseSourcePos.HasValue)
    //         {
    //             // Assign the actual sound footprint coordinates instead of cheating to find the player
    //             _lastHeardPosition = noiseSourcePos.Value;
    //             _memoryTimer = MemoryDuration;
    //             SetGlobalAlert(true);

    //             // Only distract the bandit if they aren't already actively fighting or chasing the player
    //             if (CurrentState != States[typeof(BanditChaseState)] && CurrentState != States[typeof(BanditShootState)])
    //             {
    //                 if (CurrentState != States[typeof(BanditInvestigateState)])
    //                 {
    //                     IsInvestigating = true;
                        
    //                     // Pass the position straight to the pathfinding node
    //                     SetNavTarget(noiseSourcePos.Value);
                        
    //                     // Trigger state change via State Machine
    //                     ChangeState(States[typeof(BanditInvestigateState)]);
    //                 }
    //             }
    //         }
    //     }

    //     // Keep normal visual processing alive if player isn't hidden by a mask
    //     if (Player.isWearingMask)
    //         return;

    //     Vector2 toPlayer = Player.GlobalPosition - GlobalPosition;

    //     if (_visionTargets.Contains(Player))
    //     {
    //         Vector2 dir = toPlayer.Normalized();
    //         float dot = Forward.Dot(dir);
    //         float threshold = Mathf.Cos(Mathf.DegToRad(VisionAngle * 0.5f));

    //         if (dot >= threshold)
    //         {
    //             SightRay.TargetPosition = toPlayer;
    //             SightRay.ForceRaycastUpdate();

    //             if (!SightRay.IsColliding() || SightRay.GetCollider() == Player)
    //             {
    //                 _lastSeenPosition = Player.GlobalPosition;
    //                 _memoryTimer = MemoryDuration;
    //                 SetGlobalAlert(true);
    //             }
    //         }
    //     }
    // }

    private void UpdateSuspicionMode()
    {
        if (CurrentState == States[typeof(BanditChaseState)]) { _mode = SuspicionMode.Chase; return; }
        if (CurrentState == States[typeof(BanditInvestigateState)]) { _mode = SuspicionMode.Investigate; return; }
        if (CurrentState == States[typeof(BanditAlertState)]) { _mode = SuspicionMode.Alert; return; }
        _mode = SuspicionMode.Roam;
    }

    private void UpdateMemory(float dt)
    {
        if (_memoryTimer > 0)
            _memoryTimer -= dt;
        else
        {
            _lastSeenPosition = null;
            _lastHeardPosition = null;

            if (IsGlobalAlert)
                _scanTime = 0f;
        }
    }

    public Vector2? GetMemoryTarget()
    {
        if (_lastSeenPosition.HasValue) return _lastSeenPosition;
        if (_lastHeardPosition.HasValue) return _lastHeardPosition;
        return null;
    }

    // ---------------- VISION CHECK ----------------

    public bool CanSeePlayer()
    {
        if (Player == null || Player.IsDead || !_visionTargets.Contains(Player) || Player.isWearingMask)
            return false;

        Vector2 toPlayer = Player.GlobalPosition - GlobalPosition;
        Vector2 dir = toPlayer.Normalized();
        float dot = Forward.Dot(dir);
        float threshold = Mathf.Cos(Mathf.DegToRad(VisionAngle * 0.5f));

        if (dot < threshold) return false;

        SightRay.TargetPosition = toPlayer;
        SightRay.ForceRaycastUpdate();

        return !SightRay.IsColliding() || SightRay.GetCollider() == Player;
    }

    // ---------------- ANIMATION ----------------

    private string _lastFacing = "down";

    private void UpdateAnimation()
    {
        if (Anim == null || CurrentState == States[typeof(BanditShootState)])
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

        string anim = v.LengthSquared() > 0.01f ? $"walk_{_lastFacing}" : $"idle_{_lastFacing}";

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
    }

    public void Update(double delta) => CurrentState?.Update(delta);
    public void PhysicsUpdate(double delta) => CurrentState?.PhysicsUpdate(delta);
}