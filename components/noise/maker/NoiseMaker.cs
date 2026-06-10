using Godot;
using System;

[Scene]
public partial class NoiseMaker : StaticBody2D
{
    private float _currentNoise;
    private float _targetNoise;
    private float _duration;
    [Export] private float _unitPerDecibelMultiplier = 10;
    [Export] private float _soundExpandSpeed = 150;
    [Export] private float _soundShrinkSpeed = 100;
    public bool IsMakingNoise => _currentNoise > 0;
    [Node("NoiseShape")] private CollisionShape2D _noiseCollisionShape; 

	public override void _Ready()
    {
        
    }

	public override void _Process(double delta)
    {
        var fDelta = (float)delta;
        _currentNoise = TransitionValue(_currentNoise, _targetNoise, _soundExpandSpeed, _soundShrinkSpeed, fDelta);

        if (_noiseCollisionShape.Shape is CircleShape2D shape)
        {
            shape.Radius = _currentNoise * _unitPerDecibelMultiplier;
        }

        if (_duration > 0)
        {
            _duration -= fDelta;

            if (_duration <= 0)
            {
                _duration = 0;
                _targetNoise = 0;
            }
        }
    }

    public void MakeNoise(float noiseDB, float duration = 0)
    {
        _targetNoise = noiseDB;
        _duration = duration;
    }

    public float GetReceivedNoise(Vector2 globalPosition)
    {
        float distance = GlobalPosition.DistanceTo(globalPosition);
        return Math.Clamp(_currentNoise - distance / _unitPerDecibelMultiplier, 0, _currentNoise);
    }

    float TransitionValue(float current, float target, float expandSpeed, float shrinkSpeed, float delta)
    {
        if (current == target) return current;

        double transitionSpeed = target > current  
            ? expandSpeed
            : shrinkSpeed;
        return (float)Mathf.MoveToward(current, target, transitionSpeed * delta);
    }
}
