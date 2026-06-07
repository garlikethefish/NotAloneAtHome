using System.Linq;
using Godot;
using NotAloneAtHome.Characters.Player;
using NotAloneAtHome.Components;

[Scene]
public partial class EscapeZone : Node2D
{
    [Node] public AreaDetectorComponent AreaDetectorComponent;
    [Node] public AnimationPlayer AnimationPlayer;
    [Node] public Timer Timer;
    [Node] public Label CountdownLabel;
    private Player _player;
    private bool _isFinished;
    
    public override void _Ready()
    {
        _player = GetTree().GetNodesInGroup("player").OfType<Player>().FirstOrDefault();
        AnimationPlayer.Play("disappear");
        AnimationPlayer.Play("appear");

        AreaDetectorComponent.OnBodyEntered += BodyEnter;
        AreaDetectorComponent.OnBodyExited += BodyExited;

        Timer.OneShot = true;
        Timer.Timeout += () =>
        {
            GD.Print("YOU ESCAPED!!! CONGRATS!!!");
            _isFinished = true;
        };
    }

    public override void _Process(double delta)
    {
        CountdownLabel.Text = Mathf.CeilToInt(Timer.TimeLeft).ToString();
    }

    void BodyEnter(DetectableComponent detectable)
    {
        if (detectable.GetParent() is Player)
        {
            StartEscapeTimer();
        }
    }

    void BodyExited(DetectableComponent detectable)
    {
        if (detectable.GetParent() is Player)
        {
            ResetEscapeTimer();
        }
    }

    void StartEscapeTimer()
    {
        if (_isFinished) return;
        Timer.WaitTime = 3;
        Timer.Start();
    }

    void ResetEscapeTimer()
    {
        Timer.Stop();
        CountdownLabel.Text = "";
    }
}