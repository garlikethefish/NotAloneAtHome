using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using NotAloneAtHome.Characters.Player;
using NotAloneAtHome.Components;
using NotAloneAtHome.Valuables;

[Scene]
public partial class SellZone : Node2D
{
    [Node] public AreaDetectorComponent AreaDetectorComponent;
    [Node] public AnimationPlayer AnimationPlayer;
    private List<DetectableComponent> _detectableInArea = [];
    private Player _player;
    private bool _stayVisible;
    private bool _isVisible;
    private bool _isPlayerCarryingValuable;
    public override void _Ready()
    {
        _player = GetTree().GetNodesInGroup("player").OfType<Player>().FirstOrDefault();
        AnimationPlayer.Play("disappear");

        if (_player.HasChild<CarrierComponent>(out var carrier))
        {
            carrier.OnCarriableAssigned += Appear;
            carrier.OnCarriableRemoved += Disappear;
        }

        AreaDetectorComponent.OnBodyEntered += BodyEnter;
        AreaDetectorComponent.OnBodyExited += BodyExit;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_detectableInArea.Any(item => item.GetParent() is Valuable))
        {
            if (!_stayVisible && !_isVisible)
            {
                AnimationPlayer.Play("appear");
                _stayVisible = true;
                _isVisible = true;
            }
        } 
        else
        {
            if (_stayVisible && _isVisible && !_isPlayerCarryingValuable)
            {
                AnimationPlayer.Play("disappear");
                _stayVisible = false;    
                _isVisible = false;
            }
        }
    }

    void BodyEnter(DetectableComponent detectable)
    {
        _detectableInArea.Add(detectable);

        if (detectable.GetParent() is Valuable valuable)
        {
            valuable.Sell();
        }
    }

    void BodyExit(DetectableComponent detectable)
    {
        _detectableInArea.Remove(detectable);
    }

    public void Appear(CarriableComponent carriable)
    {
        _isPlayerCarryingValuable = true;
        if (carriable.GetParent() is Valuable && !_isVisible)
        {
            _isVisible = true;
            AnimationPlayer.Play("appear");
        }
    }

    public void Disappear(CarriableComponent carriable)
    {
        _isPlayerCarryingValuable = false;
        if (carriable.GetParent() is Valuable && !_stayVisible && _isVisible)
        {
            _isVisible = false;
            AnimationPlayer.Play("disappear");
        }
    }
}