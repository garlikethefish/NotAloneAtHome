using Godot;
using NotAloneAtHome.Components;

public partial class Trash : Node2D
{
    [Export] public Sprite2D sprite2D;
    public int Health { get; private set; } = 2;

    public override void _Ready()
    {
        
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) Die();
    }

    public void Die()
    {
        GD.Print("I Died");
        QueueFree();
    }
}
