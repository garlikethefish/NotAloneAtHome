using Godot;
using System;

[Scene]
public partial class NoiseMaker : StaticBody2D
{
    private float _currentNoise;
    private float _currentNoiseArea;
    private float _targetNoiseArea;
    [Export] private float _soundExpandSpeed = 100;
    [Export] private float _soundShrinkSpeed = 50;
    public bool IsMakingNoise => _currentNoise > 0;
    [Node("NoiseShape")] private CollisionShape2D _noiseCollisionShape; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        TransitionNoiseArea((float)delta, _targetNoiseArea);
    }

    public void MakeNoise(float noiseDB, float noiseArea)
    {
        _currentNoise = noiseDB;
        _targetNoiseArea = noiseArea;
    }

    public float GetReceivedNoise(Vector2 globalPosition)
    {
        float distance = GlobalPosition.DistanceTo(globalPosition);
        // GD.Print("area radius: ", _currentNoiseArea, " distance: ", distance);
        return _currentNoise * (1f - distance / _currentNoiseArea);
    }

    public void TransitionNoiseArea(float delta, float targetRadius)
    {
        if (_currentNoiseArea == targetRadius) return;

        double transitionSpeed = targetRadius > _currentNoiseArea  
            ? _soundExpandSpeed
            : _soundShrinkSpeed;
        _currentNoiseArea = (float)Mathf.MoveToward(_currentNoiseArea, targetRadius, transitionSpeed * delta);

        if (_noiseCollisionShape.Shape is CircleShape2D shape)
        {
            shape.Radius = _currentNoiseArea;
        }
    }
}
