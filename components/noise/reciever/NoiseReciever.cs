using Godot;
using System;
using System.Collections.Generic;

public partial class NoiseReciever : Area2D
{
    public float CurrentNoise;
    private float _targetNoise;
    private float _addedNoise;
    private float _passiveMinimumNoise;
    [Export] private float _maxNoiseDisturbance;
    [Export] private float stiffness;
    [Export] private float damping;
    private float _noiseTimer = 0f;
    private float _noiseVelocity = 0f;
    private HashSet<NoiseMaker> _passiveNoiseMakers = [];

    public override void _Ready()
    {
        BodyEntered += OnAreaEnter;
        BodyExited += OnAreaExit;
    }

    public override void _Process(double delta)
    {
        _targetNoise = 0;
        _targetNoise += _maxNoiseDisturbance * GD.Randf();
        float combinedPassiveNoise = GetCombinedPassiveNoise();
        _targetNoise += combinedPassiveNoise;
        _targetNoise = Math.Clamp(_targetNoise, 0, 100);
        CurrentNoise = TransitionNoiseArea(delta, CurrentNoise, _targetNoise, stiffness, damping);
        // GD.Print("End noise: ", CurrentNoise, " Combined passive: ", combinedPassiveNoise);
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
        foreach (var noiseMaker in _passiveNoiseMakers)
        {
            combinedNoise += noiseMaker.GetReceivedNoise(GlobalPosition);
        }
        return combinedNoise;
    }

    public void AddNoise(float noise)
    {
        _addedNoise += noise;
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