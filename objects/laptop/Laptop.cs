using System;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;

public partial class Laptop : Sprite2D, IDetectable, IInteractable
{
    [Signal] public delegate void StartANewLineEventHandler();

    private ComponentHolder Holder;
    public IDetectableComponent DetectableComponent { get; set; }
    private Control       _ui;
    private Control       _vignette;
    private bool          _waitingRetry = false;
    private RandomNumberGenerator _rng = new();
    private Label  _waitCountdownText;
    private Timer  _waitCountdownTimer;
    private int    _timeToWait = 0;



    public override void _Ready()
    {
        Holder              = this.GetComponentOfType<ComponentHolder>();
        DetectableComponent = Holder.DetectableComp;

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
}