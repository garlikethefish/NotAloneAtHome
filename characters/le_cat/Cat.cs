namespace NotAloneAtHome.Characters;

using Godot;
using NotAloneAtHome.Components;

[Scene]
public partial class Cat : CharacterBody2D
{
    [Export] public float Speed = 80.0f;
    [Export] public float RoamWaitTime = 1.0f;
    [Export] public float MinX = -100.0f;
    [Export] public float MaxX = 800.0f;
    [Export] public float MinY = -100.0f;
    [Export] public float MaxY = 600.0f;
    [Export] public int DoCatTimes = 5;
    [Node] public InteractableComponent InteractableComponent;
    [Node] public NoiseMaker NoiseMaker;

    private NavigationAgent2D _navAgent;
    private float _roamTimer = 0.0f;
    private bool _hasTarget = false;
    private string _lastFacing = "front";
    private RandomNumberGenerator _rng = new();

    private AnimatedSprite2D _anim;
    private AudioStreamPlayer2D _meowSound;
    private Timer _meowTimer;

    public override void _Ready()
    {
        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _meowSound = GetNode<AudioStreamPlayer2D>("MeowSound");
        _meowTimer = GetNode<Timer>("MeowTimer");

        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        StartMeowTimer();
        InteractableComponent.OnInteractionFrom += _ =>
        {
            NoiseMaker.MakeNoise(100, 1);
            GD.Print("MJAU!");
        };
    }

    private void StartMeowTimer()
    {
        _meowTimer.Start(_rng.RandfRange(5.0f, 15.0f));
    }

    private void SetNewRoamTarget()
    {
        var randomPoint = new Vector2(
            _rng.RandfRange(MinX, MaxX),
            _rng.RandfRange(MinY, MaxY)
        );
        _navAgent.TargetPosition = randomPoint;
        _hasTarget = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        Roam((float)delta);
        Velocity = _navAgent.GetVelocity();
        MoveAndSlide();

        bool moving = Velocity.Length() > 10;

        if (moving)
        {
            if (Velocity.Y < 0) _lastFacing = "back";
            else if (Velocity.Y > 0) _lastFacing = "front";
            else _lastFacing = "side";
        }

        if (moving)
            _anim.Play(_lastFacing + "_run");
        else
            _anim.Play(_lastFacing + "_idle");

        _anim.FlipH = Velocity.X < 0;
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
            Velocity = Vector2.Zero;
            return;
        }

        var nextPos = _navAgent.GetNextPathPosition();
        var dir = GlobalPosition.DirectionTo(nextPos);
        var desiredVelocity = dir * Speed;
        Velocity = Velocity.MoveToward(desiredVelocity, 800 * delta);

        _navAgent.SetVelocity(Velocity);
        Velocity = _navAgent.GetVelocity();
    }

    private void OnMeowTimerTimeout()
    {
        _meowSound.Play();
        StartMeowTimer();
    }
}