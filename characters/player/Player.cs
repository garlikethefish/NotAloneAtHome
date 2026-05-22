namespace NotAloneAtHome.Characters.Player;

using System;
using System.Collections.Generic;
using Godot;
using NotAloneAtHome.state_machines.interfaces;
using static NotAloneAtHome.Characters.Player.States.Player;

public enum PlayerState
{
    Idle,
    Walking,
    Sprinting,
    Aiming,
    Masked,
    Frozen
}

public partial class Player : CharacterBody2D, IThrower, IInteractor, IAreaDetector, ICarrier, IStateMachine
{
    [Export] public float Speed = 100.0f;
    [Export] public float SprintMultiplier = 2.0f;
    [Export] public float MaskSpeedMultiplier = 0.6f;

    [Export] public float MaxVisionRadius = 0.35f;
    [Export] public float MinVisionRadius = 0.08f;
    [Export] public float VisionShrinkSpeed = 0.05f;
    [Export] public float AtmosphereRadius = 0.35f;
    [Export] public float VisionExpandSpeed = 0.25f;

    private float _currentRadius = 0.35f;
    public bool IsDead = false;
    public bool HasMask = false;
    public bool MaskOn = false;
    public bool IsCarryingObject = false;

    public Dictionary<Type, IState> States = new();
    private bool _banditNear = false;
    private Vector2 _banditBody;
    private float _distance;

    private Vector2 _direction = Vector2.Zero;

    private ComponentHolder _componentHolder;
    private IThrower _thrower;
    private IInteractor _interactor;
    private ICarrier _carrier;
    private CastedAreaDetectorComponent _detector;

    private AnimatedSprite2D _anim;
    private ColorRect _overlayRect;
    private AudioStreamPlayer2D _footstepSound;
    private AudioStreamPlayer2D _maskSound;
    private GpuParticles2D _breathingParticles;
    private Area2D _nearnessDetector;

    private string _lastFacing = "down";
    private bool _wait = false;
    private bool _sprinting = false;
    private bool _waitParticles = false;
    private bool _shakingCamera = false;

    public Node Node => throw new System.NotImplementedException();

    public Vector2 FacingDirection => throw new System.NotImplementedException();

    public bool IsAiming => throw new System.NotImplementedException();

    public Node2D CarryPointNode => throw new System.NotImplementedException();
    public IState CurrentState { get; private set; }

    [Signal] public delegate void ShakeCameraEventHandler();
    [Signal] public delegate void StopCameraShakeEventHandler();

    public override void _Ready()
    {
        _componentHolder = this.GetComponentOfType<ComponentHolder>();
        _thrower         = _componentHolder.Thrower;
        _interactor      = _componentHolder.Interactor;
        _carrier         = _componentHolder.Carrier;
        _detector        = (CastedAreaDetectorComponent)_componentHolder.AreaDetector;

        _anim               = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _overlayRect        = GetNode<ColorRect>("MaskOverlay/ColorRect");
        _footstepSound      = GetNode<AudioStreamPlayer2D>("FootstepSound");
        _maskSound          = GetNode<AudioStreamPlayer2D>("Breathe");
        _breathingParticles = GetNode<GpuParticles2D>("BreathingParticles");
        _nearnessDetector   = GetNode<Area2D>("BanditNearnessDetector");

        // _interactor.CanInteractCallable = CanInteract;
        // _carrier.CanCarryCallable = CanCarry;

        AddToGroup("player");

        // populating states
        States[typeof(IdleState)]   = new IdleState(this);
        States[typeof(AimingState)] = new AimingState(this);

        if (_overlayRect != null)
        {
            _overlayRect.Visible = true;
            if (_overlayRect.Material is ShaderMaterial mat)
            {
                mat.SetShaderParameter("center", new Vector2(0.5f, 0.5f));
                mat.SetShaderParameter("radius", AtmosphereRadius);
            }
        }
    }

    public override void _Process(double delta)
    {
        CurrentState.Update(delta);
        // _thrower.SetTargetDirection(GetGlobalMousePosition() - GlobalPosition);

        if (CanMove)
        {
            if (_direction != Vector2.Zero)
            {
                _carrier.FacingDirection = Velocity.Normalized();
                if (!_footstepSound.Playing && !_wait)
                    PlayFootstepSound();
            }

            if (_shakingCamera)
                EmitSignal(SignalName.StopCameraShake);

            if (_overlayRect?.Material is ShaderMaterial mat)
            {
                mat.SetShaderParameter("center", new Vector2(0.5f, 0.5f));

                if (MaskOn)
                {
                    _currentRadius = Mathf.Max(MinVisionRadius, _currentRadius - VisionShrinkSpeed * (float)delta);
                    if (!_breathingParticles.Emitting)
                        PlayBreathingParticles();
                    if (!_maskSound.Playing)
                        _maskSound.Play();
                }
                else
                {
                    _maskSound.Stop();
                    _currentRadius = Mathf.Min(AtmosphereRadius, _currentRadius + VisionExpandSpeed * (float)delta);
                }

                mat.SetShaderParameter("radius", _currentRadius);
            }
        }
        else
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

            ImmobileAnimation();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CanMove)
        {
            _direction = Vector2.Zero;
            _direction.X = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            _direction.Y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
            _direction = _direction.Normalized();

            float currentSpeed = Speed;

            if (Input.IsActionPressed("sprint"))
            {
                _sprinting = true;
                currentSpeed *= SprintMultiplier;
            }
            else
            {
                _sprinting = false;
            }

            if (MaskOn)
                currentSpeed *= MaskSpeedMultiplier;

            Velocity = _direction * currentSpeed;
            MoveAndSlide();

            UpdateAnimation(_direction);

            if (Input.IsActionJustPressed("toggle_mask"))
                ToggleMask();
        }

        if (IsDead)
            return;
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

        string maskSuffix = MaskOn ? "_mask" : "";

        _anim.Play(moving
            ? "walk_" + _lastFacing + maskSuffix
            : "idle_" + _lastFacing + maskSuffix);
    }

    private void ImmobileAnimation()
    {
        _lastFacing = "up";
        string maskSuffix = MaskOn ? "_mask" : "";
        _anim.Play("idle_" + _lastFacing + maskSuffix);
    }

    public void AddMask() => HasMask = true;

    public void ShowDefeatScreen(string reason)
    {
        var defeatScene = GD.Load<PackedScene>("res://scenes/defeat_view/DefeatScreen.tscn").Instantiate();
        GetTree().CurrentScene.AddChild(defeatScene);
        (defeatScene as DefeatScreen)?.SetDefeatReason(reason);
    }

    private async void PlayFootstepSound()
    {
        _footstepSound.Play();
        _wait = true;
        await ToSignal(GetTree().CreateTimer(_sprinting ? 0.4 : 0.5), SceneTreeTimer.SignalName.Timeout);
        _wait = false;
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

    private void ToggleMask()
    {
        if (!HasMask || (_carrier.IsCarrying && !_carrier.TryToDrop()))
            return;

        MaskOn = !MaskOn;

        if (MaskOn)
            _currentRadius = MaxVisionRadius;
    }

    public bool CanDetectLike(DetectableComponent detectable)
    {
        throw new System.NotImplementedException();
    }

    public void InteractWith(IInteractable interactable)
    {
        // if (interactable != null
        //     && interactable.MainParent is DeadThiefCloset
        //     && _carrier.Carriable != null
        //     && _carrier.Carriable.MainParent is DeadThief)
        // {
        //     return !MaskOn;
        // }

        // return !MaskOn && !_carrier.IsCarrying;
    }

    public void StartAiming()
    {
        throw new System.NotImplementedException();
    }

    public void StopAiming()
    {
        throw new System.NotImplementedException();
    }

    public void Throw(IThrowable throwable, Vector2 toPosition)
    {
        throw new System.NotImplementedException();
    }

    public void SetFacingDirection(Vector2 direction)
    {
        throw new System.NotImplementedException();
    }

    public void Pickup(ICarriable carriable)
    {
        // !MaskOn && !_carrier.IsCarrying;
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }

    public void ChangeState(IState next)
    {
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState?.Enter();
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
