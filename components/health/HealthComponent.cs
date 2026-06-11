namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
#nullable enable

[Scene]
public partial class HealthComponent : Node
{
    [Export] public int Health;
    [Node] private AnimationPlayer? AnimationPlayer;

    public event Action<int>? OnDamageTaken;
    public event Action? OnDeath;

    public override void _Ready()
    {
        base._Ready();
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        OnDamageTaken?.Invoke(amount);

        if (Health <= 0) Die();
    }

    void Die()
    {
        GD.Print("I Died");
        AnimationPlayer?.Play("disappear");
        OnDeath?.Invoke();
    }
}