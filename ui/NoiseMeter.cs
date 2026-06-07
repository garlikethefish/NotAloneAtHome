using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NoiseMeter : TextureProgressBar
{
 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        Value -= delta * 50;
        if (Input.IsActionJustPressed("sprint"))
        {
            AddNoise(20);
        }
        // GD.Print(Value);
    }

    public void AddNoise(double amount)
    {
        GD.Print("added noise");
        Value += amount;
    }
}
