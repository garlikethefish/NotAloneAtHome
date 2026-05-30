using System;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;

public partial class Laptop : Sprite2D, IDetectable, IInteractable
{
    [Signal] public delegate void StartANewLineEventHandler();

    private ComponentHolder Holder;
    private IInteractableComponent _interactableComp;
    private IDetectableComponent _detectableComp;
    private Control       _ui;
    private Control       _vignette;
    private bool          _waitingRetry = false;
    private RandomNumberGenerator _rng = new();
    private Label  _waitCountdownText;
    private Timer  _waitCountdownTimer;
    private int    _timeToWait = 0;

    public Rid Rid => _detectableComp.HandleGetRid();
    public ReactiveList<IAreaDetector> BlacklistedDetectors => _detectableComp.BlacklistedDetectors;
    public CollisionShape2D CollisionShape2D => _detectableComp.CollisionShape2D;
    public Action<IAreaDetector> OnEnteredDetectorArea { get; set; }
    public Action<IAreaDetector> OnExitedDetectorArea { get; set; }
    public Action<IAreaDetector> OnBecameDetectorPriority { get; set; }
    public Action<IAreaDetector> OnRemovedDetectorPriority { get; set; }

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        _interactableComp = Holder.InteractableComp;
        _detectableComp   = Holder.DetectableComp;

        _ui           = GetNode<Control>("ProgrammingMinigame");
        _vignette     = _ui.GetChild<Control>(0);
        _waitCountdownText  = GetNode<Label>("WaitCountdownText");
        _waitCountdownTimer = GetNode<Timer>("WaitCountdownTimer");

        // GameManager.Laptop = this;
    }

    public override void _Process(double delta)
    {
        HideOverlay();
        // if (GameManager.Player != null && GameManager.Player.IsDead)
        // {
        //     _ui.Visible              = false;
        //     GameManager.PlayerCanMove = true;
        //     _vignette.Visible        = false;
        // }
    }

    private void ShowOverlay()
    {
        // GameManager.PlayerCanMove = false;
        _ui.Visible               = true;
        _vignette.Visible         = true;
        EmitSignal(SignalName.StartANewLine);
    }

    private void HideOverlay()
    {
        if (Input.IsActionJustPressed("exit"))
        {
            _ui.Visible              = false;
            // GameManager.PlayerCanMove = true;
            _vignette.Visible        = false;
        }
    }

    // private void OnIInteractableOnInteraction(IInteractor interactor)
    // {
    //     ShowOverlay();
    // }

    // private bool CanBeInteractedWith(IInteractor interactor)
    // {
    //     // if (!_waitingRetry && !GameManager.LockedOut)
    //     //     if (GameManager.CurrentObjective == ObjectiveModel.Objective.WriteCode)
    //     //         return true;
    //     // return false;
    // }

    // private bool CanBeDetected(IProximityAreaDetector detector)
    // {
    //     return true;
    // }

    private void OnWaitCountdownTimerTimeout()
    {
        _timeToWait--;
        _waitCountdownText.Text = _timeToWait.ToString();
    }

    private async void OnProgrammingMinigameKick(bool fumbled)
    {
        if (fumbled)
        {
            // GameManager.PlayerCanMove = true;
            _vignette.Visible         = false;
            _ui.Visible               = false;
            _waitingRetry             = true;
            _timeToWait               = _rng.RandiRange(10, 20);
            _waitCountdownText.Text    = _timeToWait.ToString();
            _waitCountdownText.Visible = true;

            while (_timeToWait != 0)
            {
                _waitCountdownTimer.Start(1);
                await ToSignal(_waitCountdownTimer, Timer.SignalName.Timeout);
            }

            // ui.ResetMistakes();
            _waitingRetry             = false;
            _waitCountdownText.Visible = false;
        }
        else
        {
            // GameManager.PlayerCanMove = true;
            _vignette.Visible         = false;
            _ui.Visible               = false;
            // GameManager.LockedOut     = true;
        }
    }

    public void InteractedBy(IInteractor interactor)
    {
    
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
    }

    public void ExitAllDetectors()
    {
        _detectableComp.HandleExitAllDetectors();
    }
}