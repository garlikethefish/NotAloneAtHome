namespace NotAloneAtHome.Components;

using Godot;
using System;
#nullable enable

[Scene][Tool]
public partial class HealthComponent : Node
{
    [Export] public int Health = 1;
    [Export] private bool _exitTreeOnDeath = true;
    public event Action<int>? OnDamageTaken;
    public event Action? OnDeath;
    private bool _isDying;

    public void TakeDamage(int amount)
    {
        Health -= amount;
        OnDamageTaken?.Invoke(amount);

        if (Health <= 0) Die();
    }

    public void Die()
    {
        if (_isDying) return;

        _isDying = true;
        if (_animationPlayer == null)
        {
            OnDeath?.Invoke();
            if (_exitTreeOnDeath) GetParent().QueueFree();
            return;
        }

        _animationPlayer.AnimationFinished += (_) =>
        {
            OnDeath?.Invoke();
            if (_exitTreeOnDeath) GetParent().QueueFree();
        };
        _animationPlayer?.Play("death");
    }

    // sum fucked up shit for flexible animation player vvv

    [Export]
    public AnimationPlayer? DeathAnimationPlayer
    {
        get => _animationPlayer;
        set { _animationPlayer = value; NotifyPropertyListChanged(); }
    }
    private AnimationPlayer? _animationPlayer;
    private string _deathAnimation = "";

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        if (_animationPlayer == null)
            return properties;

        var anims = _animationPlayer.GetAnimationList();
        if (anims.Length == 0)
            return properties;

        properties.Add(new Godot.Collections.Dictionary
        {
            { "name", "DeathAnimation" },
            { "type", (int)Variant.Type.String },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", string.Join(",", anims) }
        });

        return properties;
    }

    public override Variant _Get(StringName property)
    {
        if (property == "DeathAnimation") return _deathAnimation;
        return default;
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (property == "DeathAnimation") { _deathAnimation = value.AsString(); return true; }
        return false;
    }
}