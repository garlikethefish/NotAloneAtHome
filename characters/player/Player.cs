namespace NotAloneAtHome.Characters.Player;

using System;
using System.Collections.Generic;
using Godot;
using GTweensGodot.Extensions;
using NotAloneAtHome.Components;
using NotAloneAtHome.state_machines.interfaces;

[Scene]
public partial class Player : CharacterBody2D, IStateMachine
{
    [Signal] public delegate void NewStateEventHandler(string state);
    [Signal] public delegate void CanSprintChangedEventHandler(bool value);
    [Signal] public delegate void CanToggleMaskChangedEventHandler(bool value);
    //
    // player move speed
    //
    [ExportGroup("Movement")]
    [Export] public float NormalSpeed = 100.0f;
    [Export] public float SprintMultiplier = 2.0f;
    [Export] public float MaskSpeedMultiplier = 0.6f;
    [Export] public float CarrySpeedMultiplier = 0.8f;
    float _currentSpeed = 100.0f;
    //
    // player vision
    //
    [ExportGroup("Vision")]
    [Export] public double VisionShrinkSpeed = 50;
    [Export] public double VisionExpandSpeed = 25;
    [Export] public double defaultUnmaskedVisionRadiuss = 75;
    [Export] public double defaultMaskedVisionRadiuss = 5;
    private double _targetVisionRadius = 0;
    private double _currentVisionRadius = 0.35;
    
    [ExportGroup("Debug")]
    [Export] public bool DebugState = false;
    //
    // internal flags
    //
    public bool IsAiming { get; private set; }
    public bool CanInteract { get; set; } = true;
    private bool _canSprint = false;
    public bool CanSprint
    {
        get => _canSprint;
        set
        {
            if (_canSprint == value) return;
            _canSprint = value;
            EmitSignal(SignalName.CanSprintChanged, value);
        }
    }
    private bool _canToggleMask = false;
    public bool CanToggleMask
    {
        get => _canToggleMask;
        set
        {
            if (_canToggleMask == value) return;
            _canToggleMask = value;
            EmitSignal(SignalName.CanToggleMaskChanged, value);
        }
    }
    private bool _canCarry = true;
    public bool isWearingMask = false;
    public Vector2 FacingDirection;
    public Vector2 CameraTargetPosition;

    public bool IsDead = false;
    public bool HasMask = true;
    public bool IsCarryingObject = false;

    private bool _banditNear = false;
    private Vector2 _banditBody;
    private float _distance;
    private Vector2 _moveDirection = Vector2.Zero;
    [Node("AnimatedSprite2D")] private AnimatedSprite2D _anim;
    private AudioStreamPlayer2D _footstepSound;
    private AudioStreamPlayer2D _maskSound;
    private GpuParticles2D _breathingParticles;
    private Area2D _nearnessDetector;

    private string _lastFacing = "down";
    private bool _waitBeforeWalkingSound = false;
    private bool _sprinting = false;
    private bool _waitParticles = false;
    private bool _shakingCamera = false;
    public Dictionary<Type, IState> States = [];
    public IState CurrentState { get; private set; }
    private SpringVector2 _positionSpring = new(stiffness: 100f, damping: 20f);
 
    [Node] public InteractorComponent InteractorComponent;
    [Node] public ThrowerComponent ThrowerComponent;
    [Node] public CarrierComponent CarrierComponent;
    [Node] public CastedAreaDetectorComponent CastedAreaDetectorComponent;
    [Node] public Node2D RaycastedShape;
    [Node("Camera2D")] private Camera2D _camera;
    [Signal] public delegate void ShakeCameraEventHandler();
    [Signal] public delegate void StopCameraShakeEventHandler();

    public override void _Ready()
    {
        _footstepSound      = GetNode<AudioStreamPlayer2D>("FootstepSound");
        _maskSound          = GetNode<AudioStreamPlayer2D>("Breathe");
        _breathingParticles = GetNode<GpuParticles2D>("BreathingParticles");
        _nearnessDetector   = GetNode<Area2D>("BanditNearnessDetector");
        
        // populating states
        States[typeof(IdleState)]      = new IdleState(this);
        States[typeof(AimingState)]    = new AimingState(this);
        States[typeof(CarryingState)]  = new CarryingState(this);
        States[typeof(MaskedState)]    = new MaskedState(this);
        States[typeof(SprintingState)] = new SprintingState(this);
        States[typeof(WalkingState)]   = new WalkingState(this);

        ChangeState(States[typeof(IdleState)]);

        _targetVisionRadius = defaultUnmaskedVisionRadiuss;
    }

    public override void _Process(double delta)
    {
        CurrentState?.Update(delta);
        HandleInteraction();
        HandleShaking();
        TransitionVisionRadius(delta, _targetVisionRadius);
        MoveCamera(CameraTargetPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        _moveDirection.X = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        _moveDirection.Y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        _moveDirection = _moveDirection.Normalized();

        if (!IsAiming) {
            FacingDirection = Velocity.Normalized();
            CameraTargetPosition = FacingDirection * 10;
        }

        Velocity = _moveDirection * _currentSpeed;
        MoveAndSlide();
        UpdateAnimation(_moveDirection);

        if (_shakingCamera)
            EmitSignal(SignalName.StopCameraShake);

        if (IsDead)
            return;
    }

    public void MoveCamera(Vector2 position)
    {
        _positionSpring.Tick(position, (float)GetProcessDeltaTime());
        _camera.Position = _positionSpring.Current;
    }

    public void HandleInteraction()
    {
        if (!CanInteract) return;
        if (Input.IsActionJustPressed("interact"))
        {
            var detectableComp = CastedAreaDetectorComponent.ClosestDetectable;
            if (detectableComp == null) return;

            // GD.Print($"Interacting with {detectableComp.GetParent().Name} ");
            if (detectableComp.ParentHas<CarriableComponent>(out var carriable) 
                && carriable.CanBeCarriedBy(CarrierComponent) && _canCarry
            ) {
                Pickup(carriable);
                ChangeState(States[typeof(CarryingState)]);
            }
            else if (detectableComp.ParentHas<InteractableComponent>(out var interactable)) {
                interactable.HandleInteraction(InteractorComponent);
            }
        }
    }

    public void HandleShaking()
    {
        foreach (var body in _nearnessDetector.GetOverlappingAreas())
        {
            if (body.GetParent().Name == "Bandit")
            {
                _banditBody = body.GlobalPosition;
                _banditNear = true;
                _distance = _banditBody.DistanceTo(Position);
            }
        }

        if (_banditNear && _distance <= 300)
        {
            if (!_shakingCamera)
                EmitSignal(SignalName.ShakeCamera);
            _shakingCamera = true;
        }
        else
        {
            if (_shakingCamera)
                EmitSignal(SignalName.StopCameraShake);
            _shakingCamera = false;
        }
    }

    public void TransitionVisionRadius(double delta, double targetRadius)
    {
        if (RaycastedShape == null || _currentVisionRadius == targetRadius) return;

        double transitionSpeed = targetRadius > _currentVisionRadius  
            ? VisionExpandSpeed
            : VisionShrinkSpeed;
        _currentVisionRadius =  Mathf.MoveToward(_currentVisionRadius, targetRadius, transitionSpeed * delta);

        RaycastedShape.Set("reach", _currentVisionRadius);
    }

    private void UpdateAnimation(Vector2 dir)
    {
        bool moving = dir.Length() > 0;

        if (dir.X > 0)
            _anim.FlipH = false;
        else if (dir.X < 0)
            _anim.FlipH = true;

        if (moving)
        {
            if (dir.Y < 0)       _lastFacing = "up";
            else if (dir.Y > 0)  _lastFacing = "down";
            else                 _lastFacing = "side";
        }

        string maskSuffix = isWearingMask ? "_mask" : "";

        _anim.Play(moving
            ? "walk_" + _lastFacing + maskSuffix
            : "idle_" + _lastFacing + maskSuffix);
    }

    private void ImmobileAnimation()
    {
        _lastFacing = "down";
        string maskSuffix = isWearingMask ? "_mask" : "";
        _anim.Play("idle_" + _lastFacing + maskSuffix);
    }

    public void ShowDefeatScreen(string reason)
    {
        var defeatScene = GD.Load<PackedScene>("res://scenes/defeat_view/DefeatScreen.tscn").Instantiate();
        GetTree().CurrentScene.AddChild(defeatScene);
        defeatScene.Call("set_defeat_reason", reason);
    }

    private async void PlayFootstepSound()
    {
        _footstepSound.Play();
        _waitBeforeWalkingSound = true;
        await ToSignal(GetTree().CreateTimer(_sprinting ? 0.4 : 0.5), SceneTreeTimer.SignalName.Timeout);
        _waitBeforeWalkingSound = false;
    }

    private async void PlayBreathingParticles()
    {
        _breathingParticles.Emitting = true;
        _waitParticles = true;
        await ToSignal(GetTree().CreateTimer(_sprinting ? 0.8 : 1.0), SceneTreeTimer.SignalName.Timeout);
        _waitParticles = false;
    }

    public async void Die()
    {
        EmitSignal(SignalName.StopCameraShake);
        if (IsDead) return;

        IsDead = true;
        GD.Print("You were shot!");

        Velocity = Vector2.Zero;
        SetPhysicsProcess(false);
        SetProcess(false);

        Engine.TimeScale = 0.4;
        await ToSignal(GetTree().CreateTimer(0.6), SceneTreeTimer.SignalName.Timeout);
        Engine.TimeScale = 1.0;

        ShowDefeatScreen("YOU WERE SHOT");
    }

    public void StartAiming()
    {
        ThrowerComponent.HandleStartAiming();
    }

    public void StopAiming()
    {
        ThrowerComponent.HandleStopAiming();
    }

    public void Pickup(CarriableComponent carriable)
    {
        _canCarry = false;
        CarrierComponent.HandlePickup(carriable);
    }

    public void Drop()
    {
        _canCarry = true;
        CarrierComponent.HandleDrop();
    }

    public void Throw(ThrowableComponent throwable)
    {
        _canCarry = true;
        ThrowerComponent.HandleThrow(throwable);
    }

    public void ChangeState(IState next)
    {
        var oldState = CurrentState?.GetType().Name;
        var newState = next.GetType().Name;
        
        if (DebugState) GD.Print($"[{DateTime.Now:HH:mm:ss:fff}][State] {oldState} => {newState}");

        CurrentState?.Exit();
        CurrentState = next;
        CurrentState?.Enter();

        EmitSignal(SignalName.NewState, CurrentState?.GetType().Name ?? "None");
    }

    public void Update(double delta)
    {
        CurrentState.Update(delta);
    }

    public void PhysicsUpdate(double delta)
    {
        CurrentState.PhysicsUpdate(delta);
    }
}
