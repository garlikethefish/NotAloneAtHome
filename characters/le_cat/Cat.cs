namespace NotAloneAtHome.Characters;

using Godot;
using NotAloneAtHome.Components;

[Scene]
public partial class Cat : CharacterBody2D
{
    [Export] public float Speed = 72.0f;
    [Export] public float FollowChance = 0.25f;
    [Export] public float FollowDistance = 140f;

    [ExportGroup("Audio")]
    [Export] public float MeowSoftVolumeDb = 2f;
    [Export] public float MeowLoudVolumeDb = 8f;
    [Export] public float PurrVolumeDb     = -8f;  // full-strength purr level

    [Node] public InteractableComponent InteractableComponent;
    [Node] public NoiseMaker NoiseMaker;

    private NoiseReciever _noiseReciever;
    private NavigationAgent2D _navAgent;
    private AnimatedSprite2D _anim;

    private AudioStreamPlayer2D _meowSound;
    private AudioStreamPlayer2D _purrSound;
    private AudioStreamPlayer2D _eatSound;

    private RandomNumberGenerator _rng = new();

    private CatBowl _foodBowl;
    private Node2D _player;

    private bool _isPetting;
    private float _petTimer;

    private float _vocalTimer;
    private float _purrStrength;    // 0 → 1 fade system
    private bool _isPurrFadingOut;

    private enum CatState
    {
        Idle,
        Wander,
        InvestigateNoise,
        Rest,
        GoToFood,
        Eating,
        FollowPlayer
    }

    private CatState _state = CatState.Idle;
    private float _stateTimer;
    private string _lastFacing = "front";

    // Stuck detection — sampled every 0.8 s during movement states
    private Vector2 _stuckCheckPos;
    private float _stuckTimer;

    public override void _Ready()
    {
        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _meowSound = GetNode<AudioStreamPlayer2D>("MeowSound");
        _purrSound = GetNode<AudioStreamPlayer2D>("PurrSound");
        _eatSound  = GetNodeOrNull<AudioStreamPlayer2D>("EatSound");
        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        _noiseReciever = GetNodeOrNull<NoiseReciever>("NoiseReciever");

        _foodBowl = GetTree().GetFirstNodeInGroup("food_bowl") as CatBowl;
        _player = GetTree().GetFirstNodeInGroup("player") as Node2D;

        _navAgent.PathDesiredDistance = 6f;
        _navAgent.TargetDesiredDistance = 14f;
        _navAgent.AvoidanceEnabled = true;

        // Radius drives both the navmesh margin lookup AND the RVO avoidance
        // bubble. A value that matches (or slightly exceeds) the sprite's
        // physical half-width keeps the cat from threading gaps it can't fit
        // and stops it grazing wall corners.
        _navAgent.Radius = 14f;

        // MaxSpeed must be set for RVO to scale avoidance forces correctly.
        // Use a ceiling above normal Speed so it stays accurate even when
        // multipliers are applied (e.g. GoToFood * 1.2).
        _navAgent.MaxSpeed = Speed * 1.5f;

        // How far ahead (in seconds) the agent looks for other agents /
        // obstacles. Longer horizons = smoother, earlier course corrections.
        _navAgent.TimeHorizonAgents = 1.5f;
        _navAgent.TimeHorizonObstacles = 2.5f;

        // Scan a wider neighbourhood so furniture / NavigationObstacle2D
        // nodes are picked up before the cat is already on top of them.
        _navAgent.NeighborDistance = 100f;
        _navAgent.MaxNeighbors = 12;

        InteractableComponent.OnInteractionFrom += _ =>
        {
            NoiseMaker.MakeNoise(120, 1);
            StartPetting(3f);
            // No meow on pet — the purr takes over
        };

        // Hook StopPetting() to whatever release/end event your InteractableComponent
        // exposes, or call cat.StopPetting() directly from the player script on key-up.

        StartMeowTimer();
        EnterIdle();
        _stuckCheckPos = GlobalPosition;
    }

    // ---------------- PETTING ----------------

    public void StartPetting(float duration)
    {
        _isPetting = true;
        _isPurrFadingOut = false;   // cancel any lingering fade
        _petTimer = duration;

        _state = CatState.Idle;
        _navAgent.Velocity = Vector2.Zero;

        _purrStrength = 1f;

        // Delay the next meow until well after petting ends
        _vocalTimer = duration + _rng.RandfRange(3f, 6f);

        if (!_purrSound.Playing)
            _purrSound.Play();
    }

    private void HandlePetting(float dt)
    {
        _petTimer -= dt;

        Velocity = Vector2.Zero;
        _navAgent.Velocity = Vector2.Zero;

        _purrStrength = Mathf.MoveToward(_purrStrength, 1f, dt * 4f);

        // gentle continuous noise (calm presence)
        NoiseMaker?.MakeNoise(8f * _purrStrength, dt);

        _purrSound.VolumeDb = Mathf.Lerp(-60f, PurrVolumeDb, _purrStrength);

        _anim.Play($"{_lastFacing}_idle");

        if (_petTimer <= 0)
        {
            _isPetting = false;
            _isPurrFadingOut = true;    // hand off to fade-out, don't cut instantly
        }
    }

    private void HandlePurrFadeOut(float dt)
    {
        // ~0.15-second quick fade to silence
        _purrStrength = Mathf.MoveToward(_purrStrength, 0f, dt * 7f);
        _purrSound.VolumeDb = Mathf.Lerp(-60f, PurrVolumeDb, _purrStrength);

        if (_purrStrength <= 0f)
        {
            _purrSound.Stop();
            _isPurrFadingOut = false;
        }
    }

    // Call this from the player when E is released to cut the purr right away
    public void StopPetting()
    {
        if (!_isPetting && !_isPurrFadingOut) return;
        _isPetting = false;
        _isPurrFadingOut = true;
    }

    // ---------------- STUCK DETECTION ----------------

    private void CheckIfStuck(float dt)
    {
        // Only care during states where the cat should be moving
        if (_state is not (CatState.Wander or CatState.InvestigateNoise or
                           CatState.GoToFood or CatState.FollowPlayer))
        {
            _stuckTimer    = 0f;
            _stuckCheckPos = GlobalPosition;
            return;
        }

        _stuckTimer += dt;
        if (_stuckTimer < 0.8f) return;

        float moved = GlobalPosition.DistanceTo(_stuckCheckPos);

        if (moved < 6f)
            Unstick();

        _stuckCheckPos = GlobalPosition;
        _stuckTimer    = 0f;
    }

    private void Unstick()
    {
        // Pick a random direction and set a new nav target far enough away
        // that the agent has to route around whatever is blocking it.
        float angle = _rng.RandfRange(0f, Mathf.Tau);
        float dist  = _rng.RandfRange(100f, 200f);

        _navAgent.TargetPosition = GlobalPosition +
            new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

        _navAgent.Velocity = Vector2.Zero;
        _state = CatState.Wander;
    }

    // ---------------- LOOP ----------------

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (_isPurrFadingOut)
            HandlePurrFadeOut(dt);

        if (_isPetting)
        {
            HandlePetting(dt);
            MoveAndSlide();
            return;
        }

        HandleVocalization(dt);
        CheckIfStuck(dt);
        EvaluateNoise();
        EvaluateHunger();
        EvaluateFollowPlayer();

        switch (_state)
        {
            case CatState.Idle: HandleIdle(dt); break;
            case CatState.Wander: HandleWander(); break;
            case CatState.InvestigateNoise: HandleInvestigate(); break;
            case CatState.Rest: HandleRest(dt); break;
            case CatState.GoToFood: HandleGoToFood(); break;
            case CatState.Eating: HandleEating(dt); break;
            case CatState.FollowPlayer: HandleFollowPlayer(); break;
        }

        Velocity = _navAgent.GetVelocity();
        MoveAndSlide();

        UpdateAnimationFromVelocity(Velocity);
    }

    // ---------------- VOCAL SYSTEM (ACTIVE BEHAVIOR) ----------------

    private void HandleVocalization(float dt)
    {
        _vocalTimer -= dt;

        if (_vocalTimer <= 0f)
        {
            PlayMeowSoft();

            // meows now generate noise in world
            NoiseMaker?.MakeNoise(35f, 0.5f);

            _vocalTimer = _rng.RandfRange(3f, 7f);
        }
    }

    private void StartMeowTimer()
    {
        _vocalTimer = _rng.RandfRange(2.5f, 5f);
    }

    private void PlayMeowSoft()
    {
        _meowSound.VolumeDb = MeowSoftVolumeDb;
        _meowSound.PitchScale = _rng.RandfRange(0.95f, 1.1f);
        _meowSound.Play();
    }

    private void PlayMeowLoud()
    {
        _meowSound.VolumeDb = MeowLoudVolumeDb;
        _meowSound.PitchScale = 1.2f;
        _meowSound.Play();

        NoiseMaker?.MakeNoise(60f, 0.8f);
    }

    // ---------------- FOLLOW PLAYER ----------------

    private void EvaluateFollowPlayer()
    {
        if (_player == null) return;
        if (_state != CatState.Idle) return;

        if (_rng.Randf() < FollowChance)
            _state = CatState.FollowPlayer;
    }

    private void HandleFollowPlayer()
    {
        if (_player == null)
        {
            EnterIdle();
            return;
        }

        float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Hard stop once inside the follow radius
        if (dist <= FollowDistance)
        {
            _navAgent.Velocity = Vector2.Zero;
            return;
        }

        // Ramp speed down over the last 80 px before the stop radius so the
        // cat decelerates smoothly instead of snapping between full-speed and
        // zero, which is what causes the oscillation / jitter.
        const float slowdownRange = 80f;
        float speedFactor = Mathf.Clamp((dist - FollowDistance) / slowdownRange, 0f, 1f);

        // Target a spot at FollowDistance behind the player so we never try to
        // walk onto the player's exact tile.
        Vector2 toPlayer = GlobalPosition.DirectionTo(_player.GlobalPosition);
        _navAgent.TargetPosition = _player.GlobalPosition - toPlayer * FollowDistance;

        Vector2 next = _navAgent.GetNextPathPosition();
        Vector2 dir = GlobalPosition.DirectionTo(next);

        _navAgent.Velocity = dir * Speed * speedFactor;
    }

    // ---------------- FOOD ----------------

    private void EvaluateHunger()
    {
        // If we missed it at startup, grab it now!
        if (_foodBowl == null)
        {
            _foodBowl = GetTree().GetFirstNodeInGroup("food_bowl") as CatBowl;
        }

        // If there's still no bowl in the scene, or it has no food, ignore it
        if (_foodBowl == null || !_foodBowl.HasFood)
            return;

        // FIX 1: If we are already walking to the food or eating, 
        // DO NOT reset the target position! This prevents the every-frame path clearing bug.
        if (_state == CatState.GoToFood || _state == CatState.Eating)
            return;

        // FIX 2: Allow food to interrupt lower-priority states (Idle, Wander, Rest, FollowPlayer).
        // We only guard against critical/urgent states like investigating a scary noise.
        if (_state == CatState.InvestigateNoise)
            return;

        // Success: Transition to the state and set the path target EXACTLY ONCE
        _state = CatState.GoToFood;
        _navAgent.TargetPosition = _foodBowl.GlobalPosition;
    }

    private void HandleGoToFood()
    {
        if (_navAgent.IsNavigationFinished())
        {
            _state = CatState.Eating;
            _stateTimer = 4f;
            _eatSound?.Play();
            return;
        }

        Vector2 next = _navAgent.GetNextPathPosition();
        Vector2 dir = GlobalPosition.DirectionTo(next);

        _navAgent.Velocity = dir * (Speed * 1.2f);
    }

    private void HandleEating(float dt)
    {
        _stateTimer -= dt;
        _navAgent.Velocity = Vector2.Zero;

        if (_stateTimer <= 0)
        {
            if (_eatSound != null && _eatSound.Playing)
                _eatSound.Stop();

            _foodBowl?.ConsumeFood();
            EnterIdle();
        }
    }

    // ---------------- NOISE ----------------

    private void EvaluateNoise()
    {
        if (_noiseReciever == null)
            return;

        if (_noiseReciever.CurrentNoise > 30f)
        {
            _state = CatState.InvestigateNoise;

            _navAgent.TargetPosition = GlobalPosition + Vector2.Left * 180f;
            _stateTimer = 5f;
        }
    }

    private void HandleInvestigate()
    {
        if (_navAgent.IsNavigationFinished())
        {
            EnterIdle();
            return;
        }

        Vector2 next = _navAgent.GetNextPathPosition();
        Vector2 dir = GlobalPosition.DirectionTo(next);

        _navAgent.Velocity = dir * (Speed * 1.3f);
    }

    // ---------------- BASIC STATES (MORE ACTIVE) ----------------

    private void EnterIdle()
    {
        _state = CatState.Idle;
        _stateTimer = _rng.RandfRange(0.6f, 1.8f); // was 1.2–3.5
        _navAgent.Velocity = Vector2.Zero;
    }

    private void HandleIdle(float dt)
    {
        _stateTimer -= dt;

        if (_stateTimer <= 0)
        {
            float roll = _rng.Randf();

            if (roll < 0.85f)   // was 0.65
                EnterWander();
            else
                EnterRest();
        }
    }

    private void EnterWander()
    {
        _state = CatState.Wander;

        // Polar coordinates give a uniform disc of destinations rather than
        // a square, so the cat picks directions more evenly and is less likely
        // to aim straight at a wall corner.  Add a small random angular nudge
        // away from the current facing so consecutive wanders feel varied.
        float angle   = _rng.RandfRange(0f, Mathf.Tau);
        float distance = _rng.RandfRange(80f, 200f);
        Vector2 target = GlobalPosition +
            new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

        _navAgent.TargetPosition = target;
    }

    private void HandleWander()
    {
        if (_navAgent.IsNavigationFinished())
        {
            EnterIdle();
            return;
        }

        Vector2 next = _navAgent.GetNextPathPosition();
        Vector2 dir = GlobalPosition.DirectionTo(next);

        _navAgent.Velocity = dir * Speed;
    }

    private void EnterRest()
    {
        _state = CatState.Rest;
        _stateTimer = _rng.RandfRange(1f, 2.5f); // was 2–5
        _navAgent.Velocity = Vector2.Zero;
    }

    private void HandleRest(float dt)
    {
        _stateTimer -= dt;

        if (_stateTimer <= 0)
            EnterIdle();
    }

    // ---------------- ANIMATION ----------------

    private void UpdateAnimationFromVelocity(Vector2 velocity)
    {
        bool moving = velocity.LengthSquared() > 10f;

        if (moving)
        {
            if (Mathf.Abs(velocity.X) > Mathf.Abs(velocity.Y))
            {
                _lastFacing = "side";
                _anim.FlipH = velocity.X < 0;
            }
            else if (velocity.Y < 0)
            {
                _lastFacing = "back";
                _anim.FlipH = false;
            }
            else
            {
                _lastFacing = "front";
                _anim.FlipH = false;
            }
        }

        string anim = moving ? $"{_lastFacing}_run" : $"{_lastFacing}_idle";

        if (_anim.Animation != anim)
            _anim.Play(anim);
    }
}