using System;
using System.Collections.Generic;
using NotAloneAtHome.state_machines.interfaces;
using NotAloneAtHome.Characters.Player;
using NotAloneAtHome.Scripts.Globals;
using Godot;

[Tool]
public partial class Bandit : CharacterBody2D, IStateMachine
{
    [ExportGroup("Movement")]
    [Export] public float Speed = 75f;
    [Export] public float ChaseSpeed = 110f;
    [Export] public float Acceleration = 6f;
    [Export] public float Deceleration = 8f;
    [Export] public float TurnSmoothing = 10f;

    [ExportGroup("Vision")]
    private float _visionRange = 220f;
    [Export] public float VisionRange 
    { 
        get => _visionRange; 
        set { _visionRange = value; QueueRedraw(); } 
    }

    private float _visionAngle = 70f;
    [Export] public float VisionAngle 
    { 
        get => _visionAngle; 
        set { _visionAngle = value; QueueRedraw(); } 
    }

    [Export] public float VisionConeOffset = 12f;
    
    private Color _visionConeColor = new Color(1f, 0f, 0f, 0.25f);
    [Export] public Color VisionConeColor
    {
        get => _visionConeColor;
        set { _visionConeColor = value; QueueRedraw(); _shaderNeedsUpdate = true;}
    }

    [ExportGroup("Combat")]
    [Export] public float ShootCooldown = 1.2f;
    [Export] public float ShootDistance = 200f;

    [ExportGroup("Hearing")]
    private float _hearingRange = 260f;
    [Export] public float HearingRange 
    { 
        get => _hearingRange; 
        set { _hearingRange = value; QueueRedraw(); } 
    }
    [Export] public float SprintNoiseMultiplier = 1.8f;
    [Export] public float HearingThreshold = 30f;

    [ExportGroup("Suspicion")]
    [Export] public float RoamingSuspicionRate = 0.5f;
    [Export] public float SeenSuspicionRate = 6f;
    [Export] public float ChaseSuspicionRate = 12f;
    [Export] public float MaskReductionMultiplier = 0.4f;

    [ExportGroup("Performance")]
    [Export] public int PerceptionApiTicks { get; set; } = 10; 

    [ExportGroup("Debug")]
    [Export] public bool DebugVision = false;

    public Dictionary<Type, IState> States { get; set; } = new();
    public IState CurrentState { get; private set; }

    public Player Player { get; private set; }
    public NavigationAgent2D Nav { get; private set; }
    public RayCast2D SightRay { get; private set; }
    public AnimatedSprite2D Anim { get; private set; }
    public AudioStreamPlayer2D Gunshot { get; private set; }
    
    private Node2D _visionConeParent;
    private Polygon2D _visionPolygon;
    private ShaderMaterial _visionShader;

    private NoiseReciever _noiseReceiver;
    private Area2D _visionArea;
    private readonly HashSet<Node2D> _visionTargets = [];

    private enum SuspicionMode { Roam, Alert, Investigate, Chase }
    private SuspicionMode _mode;

    public float ShootTimer { get; set; }
    public bool IsGlobalAlert { get; private set; }
    public bool IsInvestigating { get; set; }

    public Vector2 Forward { get; private set; } = Vector2.Down;
    private Vector2 _smoothedForward = Vector2.Down;
    private Vector2 _velocity = Vector2.Zero;

    private Vector2? _lastSeenPosition;
    private Vector2? _lastHeardPosition;
    private float _memoryTimer;
    private const float MemoryDuration = 6f;

    private Vector2 _stuckCheckPos;
    private float _stuckTimer;
    private readonly RandomNumberGenerator _rng = new();

    private double _perceptionTimer;
    private bool _shaderNeedsUpdate = true;
    private bool _lastMaskState = false;

    private void OnVisionEnter(Node2D body) => _visionTargets.Add(body);
    private void OnVisionExit(Node2D body) => _visionTargets.Remove(body);

    public override void _Ready()
    {
        Anim           = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        Nav            = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        SightRay       = GetNodeOrNull<RayCast2D>("SightRay");
        Gunshot        = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        _noiseReceiver = GetNodeOrNull<NoiseReciever>("NoiseReciever");
        _visionArea    = GetNodeOrNull<Area2D>("VisionArea");

        if (_visionArea != null)
        {
            _visionArea.BodyEntered += OnVisionEnter;
            _visionArea.BodyExited  += OnVisionExit;
        }

        _visionConeParent = GetNodeOrNull<Node2D>("VisionCone");
        if (_visionConeParent != null)
        {
            _visionPolygon = _visionConeParent.GetNodeOrNull<Polygon2D>("Polygon2D");
            if (_visionPolygon != null)
            {
                _visionPolygon.Material = _visionPolygon.Material?.Duplicate() as ShaderMaterial;
                _visionShader = _visionPolygon.Material as ShaderMaterial;
                _shaderNeedsUpdate = true;

                // BUG FIX: Forces Godot to pass UV mapping variables to your fragment shader
                if (_visionPolygon.Texture == null)
                {
                    _visionPolygon.Texture = new PlaceholderTexture2D();
                }
            }
        }

        if (SightRay != null) SightRay.AddException(this);
        if (Engine.IsEditorHint()) return;

        Player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (Nav != null)
        {
            Nav.PathDesiredDistance  = 6f;
            Nav.TargetDesiredDistance = 16f;
            Nav.AvoidanceEnabled     = true;
            Nav.Radius               = 16f;
            Nav.MaxSpeed             = ChaseSpeed * 1.5f;
        }

        _stuckCheckPos = GlobalPosition;

        States[typeof(BanditRoamState)]        = new BanditRoamState(this);
        States[typeof(BanditChaseState)]       = new BanditChaseState(this);
        States[typeof(BanditInvestigateState)] = new BanditInvestigateState(this);
        States[typeof(BanditShootState)]       = new BanditShootState(this);
        States[typeof(BanditAlertState)]       = new BanditAlertState(this);

        ChangeState(States[typeof(BanditRoamState)]);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        float dt = (float)delta;
        CurrentState?.PhysicsUpdate(delta);

        _perceptionTimer += delta;
        double tickRate = 1.0 / Math.Max(1, PerceptionApiTicks);
        if (_perceptionTimer >= tickRate)
        {
            UpdatePerception();
            _perceptionTimer = 0;
        }

        UpdateSuspicion(dt);
        UpdateMovement(dt);
        CheckIfStuck(dt);

        Velocity = _velocity;
        MoveAndSlide();

        UpdateForward(dt);
        UpdateMemory(dt);
        UpdateAnimation();
        UpdateVisionPolygon();
        UpdateShaderUniforms();
    }

    public override void _Draw()
    {
        if (!Engine.IsEditorHint() && !DebugVision) return;

        DrawArc(Vector2.Zero, VisionRange, 0, Mathf.Tau, 64, new Color(1, 0, 0, 0.5f), 2f);

        float rad = Mathf.DegToRad(VisionAngle * 0.5f);
        Vector2 vLeft = Vector2.Down.Rotated(-rad) * VisionRange;
        Vector2 vRight = Vector2.Down.Rotated(rad) * VisionRange;
        DrawLine(Vector2.Zero, vLeft, new Color(1, 0, 0, 0.5f), 2f);
        DrawLine(Vector2.Zero, vRight, new Color(1, 0, 0, 0.5f), 2f);

        DrawArc(Vector2.Zero, HearingRange, 0, Mathf.Tau, 64, new Color(0, 1, 1, 0.3f), 1f);
        DrawArc(Vector2.Zero, ShootDistance, 0, Mathf.Tau, 32, new Color(1, 0.5f, 0, 0.4f), 1f, true);
    }

    private void UpdatePerception()
    {
        if (Player == null || Player.IsDead) return;

        if (CanSeePlayer())
        {
            _lastSeenPosition = Player.GlobalPosition;
            _memoryTimer      = MemoryDuration;
            SetGlobalAlert(true);

            if (CurrentState != States[typeof(BanditShootState)])
            {
                float dist = GlobalPosition.DistanceTo(Player.GlobalPosition);
                if (dist <= ShootDistance)
                    ChangeState(States[typeof(BanditShootState)]);
                else if (CurrentState != States[typeof(BanditChaseState)])
                    ChangeState(States[typeof(BanditChaseState)]);
            }
        }

        if (_noiseReceiver == null) return;

        if (_noiseReceiver.CurrentNoise > HearingThreshold)
        {
            Vector2? noisePos = _noiseReceiver.GetLoudestNoisePosition();
            if (!noisePos.HasValue) return;

            _lastHeardPosition = noisePos.Value;
            _memoryTimer       = MemoryDuration;
            SetGlobalAlert(true);

            if (CurrentState == States[typeof(BanditChaseState)] || CurrentState == States[typeof(BanditShootState)])
                return;

            SetNavTarget(noisePos.Value);
            IsInvestigating = true;

            if (CurrentState != States[typeof(BanditInvestigateState)])
                ChangeState(States[typeof(BanditInvestigateState)]);
        }
    }

    private void UpdateShaderUniforms()
    {
        if (_visionShader == null) return;

        if (_shaderNeedsUpdate)
        {
            _visionShader.SetShaderParameter("cone_color", VisionConeColor);
            _shaderNeedsUpdate = false;
        }

        bool pWearingMask = Player?.isWearingMask ?? false;
        if (pWearingMask != _lastMaskState)
        {
            _visionShader.SetShaderParameter("is_player_wearing_mask", pWearingMask);
            _lastMaskState = pWearingMask;
        }
    }

    private void UpdateVisionPolygon()
    {
        if (_visionPolygon == null || !Visible) return;

        if (Engine.IsEditorHint()) Forward = Vector2.Down; 

        const int rays = 60; 
        List<Vector2> points = new() { Vector2.Zero };
        List<Vector2> uvs = new() { new Vector2(0.5f, 0.5f) }; 

        float start = -Mathf.DegToRad(VisionAngle * 0.5f);
        float end = Mathf.DegToRad(VisionAngle * 0.5f);
        var space = GetWorld2D().DirectSpaceState;

        for (int i = 0; i <= rays; i++)
        {
            float t = (float)i / rays;
            float angle = Mathf.Lerp(start, end, t);
            Vector2 dir = Forward.Rotated(angle);
            Vector2 from = GlobalPosition;
            Vector2 to = from + dir * VisionRange;

            PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(from, to);
            query.Exclude = [GetRid()];
            query.CollideWithAreas = false;

            var result = space.IntersectRay(query);
            Vector2 hitPoint = result.Count > 0 ? (Vector2)result["position"] : to;

            points.Add(ToLocal(hitPoint));

            float edgeDist = hitPoint.DistanceTo(from) / VisionRange;
            Vector2 uvDir = Vector2.Up.Rotated(angle); 
            uvs.Add(new Vector2(0.5f, 0.5f) + uvDir * edgeDist * 0.5f);
        }

        _visionPolygon.Polygon = points.ToArray();
        
        // FIX: Replaced .Uv with the correct upper-case property .UV
        _visionPolygon.UV = uvs.ToArray(); 
    }

    private void UpdateMovement(float dt)
    {
        Vector2 targetVelocity = Vector2.Zero;
        if (Nav != null && !Nav.IsNavigationFinished())
        {
            targetVelocity = GlobalPosition.DirectionTo(Nav.GetNextPathPosition()) * (IsGlobalAlert ? ChaseSpeed : Speed);
            Nav.Velocity = _velocity;
        }
        float accel = targetVelocity.Length() > _velocity.Length() ? Acceleration : Deceleration;
        _velocity = _velocity.Lerp(targetVelocity, accel * dt);
    }

    private void CheckIfStuck(float dt)
    {
        if (Nav == null || Nav.IsNavigationFinished()) { _stuckTimer = 0f; _stuckCheckPos = GlobalPosition; return; }
        _stuckTimer += dt;
        if (_stuckTimer < 0.8f) return;
        if (GlobalPosition.DistanceTo(_stuckCheckPos) < 6f) Unstick();
        _stuckCheckPos = GlobalPosition; _stuckTimer = 0f;
    }

    private void Unstick()
    {
        if (Nav == null) return;
        float nudgeAngle = _rng.RandfRange(0f, Mathf.Tau);
        Nav.TargetPosition += new Vector2(Mathf.Cos(nudgeAngle), Mathf.Sin(nudgeAngle)) * 15f;
        _velocity = Vector2.Zero;
    }

    private void UpdateForward(float dt)
    {
        float t = 1f - Mathf.Exp(-TurnSmoothing * dt);
        if (Nav != null && !Nav.IsNavigationFinished())
        {
            Forward = Forward.Lerp(GlobalPosition.DirectionTo(Nav.GetNextPathPosition()), t).Normalized();
            if (_velocity.LengthSquared() > 0.001f) _smoothedForward = _smoothedForward.Lerp(_velocity.Normalized(), t).Normalized();
        }
        else if (_velocity.LengthSquared() > 0.001f)
        {
            Vector2 velDir = _velocity.Normalized(); Forward = Forward.Lerp(velDir, t).Normalized(); _smoothedForward = _smoothedForward.Lerp(velDir, t).Normalized();
        }
    }

    private void UpdateSuspicion(float dt)
    {
        if (GameManager.Instance == null || Player == null) return;
        UpdateSuspicionMode();
        float rate = _mode switch { SuspicionMode.Chase => ChaseSuspicionRate, SuspicionMode.Investigate => SeenSuspicionRate * 1.5f, SuspicionMode.Alert => SeenSuspicionRate, _ => RoamingSuspicionRate };
        if (Player.isWearingMask) rate *= MaskReductionMultiplier;
        GameManager.Instance.AddSuspicion(rate * dt);
    }

    private void UpdateSuspicionMode()
    {
        if (CurrentState == States[typeof(BanditChaseState)]) { _mode = SuspicionMode.Chase; return; }
        if (CurrentState == States[typeof(BanditInvestigateState)]) { _mode = SuspicionMode.Investigate; return; }
        if (CurrentState == States[typeof(BanditAlertState)]) { _mode = SuspicionMode.Alert; return; }
        _mode = SuspicionMode.Roam;
    }

    private void UpdateMemory(float dt)
    {
        if (_memoryTimer > 0f) { _memoryTimer -= dt; return; }
        if (_lastSeenPosition.HasValue || _lastHeardPosition.HasValue) { _lastSeenPosition = null; _lastHeardPosition = null; SetGlobalAlert(false); }
    }

    public Vector2? GetMemoryTarget() => _lastSeenPosition ?? _lastHeardPosition;

    public bool CanSeePlayer()
    {
        if (Player == null || Player.IsDead || !_visionTargets.Contains(Player) || Player.isWearingMask || SightRay == null) return false;
        Vector2 toPlayer = Player.GlobalPosition - GlobalPosition;
        if (Forward.Dot(toPlayer.Normalized()) < Mathf.Cos(Mathf.DegToRad(VisionAngle * 0.5f))) return false;
        SightRay.TargetPosition = toPlayer; SightRay.ForceRaycastUpdate();
        return !SightRay.IsColliding() || SightRay.GetCollider() == Player;
    }

    private string _lastFacing = "down";
    private void UpdateAnimation()
    {
        if (Anim == null || CurrentState == States[typeof(BanditShootState)]) return;
        Vector2 v = Velocity;
        if (Mathf.Abs(v.X) > Mathf.Abs(v.Y)) { _lastFacing = "side"; Anim.FlipH = v.X < 0; }
        else if (v.Y < 0) _lastFacing = "up"; else if (v.Y > 0) _lastFacing = "down";
        string anim = v.LengthSquared() > 0.01f ? $"walk_{_lastFacing}" : $"idle_{_lastFacing}";
        if (Anim.Animation != anim) Anim.Play(anim);
    }

    public void SetNavTarget(Vector2 pos) { if (Nav != null) Nav.TargetPosition = pos; }
    public void SetGlobalAlert(bool value) => IsGlobalAlert = value;
    public void ChangeState(IState next) { CurrentState?.Exit(); CurrentState = next; CurrentState?.Enter(); }
    public void Update(double delta) => CurrentState?.Update(delta);
    public void PhysicsUpdate(double delta) => CurrentState?.PhysicsUpdate(delta);
}