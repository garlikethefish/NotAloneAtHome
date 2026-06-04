namespace NotAloneAtHome.Components;

using Godot;
using System;
using System.Collections.Generic;
#nullable enable

public partial class HealthComponent : Node
{
    [Export] public int Health;

    public event Action<int>? OnDamageTaken;
    public event Action? OnDeath;

    public void TakeDamage(int amount)
    {
        Health -= amount;
        OnDamageTaken?.Invoke(amount);

        if (Health <= 0) Die();
    }

    void Die()
    {
        GD.Print("I Died");
        OnDeath?.Invoke();
    }
}