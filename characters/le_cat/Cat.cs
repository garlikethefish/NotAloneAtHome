using System;
using System.Collections.Generic;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;
using NotAloneAtHome.Tasks;
using NotAloneAtHome.Tasks.WaterPlantsTask;

public partial class Cat : CharacterBody2D, IInteractable, IDetectable
{
    [Export] public float Speed = 80.0f;
    [Export] public float RoamWaitTime = 1.0f;
    [Export] public float MinX = -100.0f;
    [Export] public float MaxX = 800.0f;
    [Export] public float MinY = -100.0f;
    [Export] public float MaxY = 600.0f;
    [Export] public int DoCatTimes = 5;

    private NavigationAgent2D _navAgent;
    private float _roamTimer = 0.0f;
    private bool _hasTarget = false;
    private string _lastFacing = "front";
    private RandomNumberGenerator _rng = new();

    private AnimatedSprite2D _anim;
    private AudioStreamPlayer2D _meowSound;
    private Timer _meowTimer;
    private ComponentHolder Holder;
    private IInteractableComponent _interactable;

    public IDetectableComponent DetectableComp { get; set; }

    public Rid Rid => DetectableComp.HandleGetRid();

    public ReactiveList<IAreaDetector> BlacklistedDetectors => DetectableComp.BlacklistedDetectors;

    public CollisionShape2D CollisionShape2D => DetectableComp.CollisionShape2D;

    public Action<IAreaDetector> OnEnteredDetectorArea { get; set; }
    public Action<IAreaDetector> OnExitedDetectorArea { get; set; }
    public Action<IAreaDetector> OnBecameDetectorPriority { get; set; }
    public Action<IAreaDetector> OnRemovedDetectorPriority { get; set; }

    public override void _Ready()
    {
        Holder = GetNode<ComponentHolder>("ComponentHolder");
        _interactable = Holder.InteractableComp;
        DetectableComp = Holder.DetectableComp;
        

        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _meowSound = GetNode<AudioStreamPlayer2D>("MeowSound");
        _meowTimer = GetNode<Timer>("MeowTimer");

        _navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        StartMeowTimer();
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

    public void InteractedBy(IInteractor interactor)
    {
        DoCatTimes--;
        if (DoCatTimes <= 0)
        {
            // complete task
        }
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return TaskManager.Instance.CurrentTask is WaterPlantsTask;
    }

    public void ExitAllDetectors()
    {
        DetectableComp.HandleExitAllDetectors();
    }
}