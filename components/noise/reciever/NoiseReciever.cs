using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Scene][Tool]
public partial class NoiseReciever : Area2D
{
    public float CurrentNoise;
    private float _targetNoise;
    private float _addedNoise;
    private float _passiveMinimumNoise;
    [ExportGroup("Noise transition")]
    [Export] private float _maxNoiseDisturbance;
    [Export] private float stiffness;
    [Export] private float damping;

    [ExportGroup("Area sizes")]
    private float _pickupRadius;
    [Export]
    private float PickupRadius
    {
        get => _pickupRadius;
        set
        {
            _pickupRadius = value;
            var shape = GetNodeOrNull<CollisionShape2D>("PickupRange");
            if (shape?.Shape is CircleShape2D circle) circle.Radius = _pickupRadius;
        }
    }
    private float _noiseVelocity = 0f;
    private HashSet<NoiseMaker> _passiveNoiseMakers = [];

    public override void _Ready()
    {
        BodyEntered += OnAreaEnter;
        BodyExited += OnAreaExit;
    }

    public override void _Process(double delta)
    {
        _targetNoise = GetCombinedPassiveNoise();
        _targetNoise += _maxNoiseDisturbance * GD.Randf();
        _targetNoise = Math.Clamp(_targetNoise, 0, 100);
        CurrentNoise = TransitionNoiseArea(delta, CurrentNoise, _targetNoise, stiffness, damping);
    }

    private void OnAreaEnter(Node2D body)
    {
        if (body is not NoiseMaker maker) return;
        _passiveNoiseMakers.Add(maker);
    }

    private void OnAreaExit(Node2D body)
    {
        if (body is not NoiseMaker maker) return;
        _passiveNoiseMakers.Remove(maker);
    }

    float GetCombinedPassiveNoise()
    {
        float combinedNoise = 0f;
        foreach (var noiseMaker in _passiveNoiseMakers.ToList())
        {
            combinedNoise += noiseMaker.GetReceivedNoise(GlobalPosition);
        }
        return combinedNoise;
    }

    public float TransitionNoiseArea(
        double delta,
        float currentNoise,
        float targetNoise,
        float stiffness,
        float damping
    ) {
        if (stiffness == 0f) return targetNoise;
        float force = (targetNoise - currentNoise) * stiffness;
        _noiseVelocity += (force - _noiseVelocity * damping) * (float)delta;
        return currentNoise + _noiseVelocity * (float)delta;
    }
}