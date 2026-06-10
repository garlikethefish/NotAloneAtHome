using Godot;
using NotAloneAtHome.Tasks;
using System;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print($"Starting game on scene {GetTree().CurrentScene.Name}");
		TaskManager.Instance.AddTask(new FeedCatTask(GetTree()));
		TaskManager.Instance.AddTask(new HideDeadBanditTask(GetTree()));
		TaskManager.Instance.AddTask(new CollectTrashTask(GetTree(), 4));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
