using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;
using System;

[Scene]
public partial class CatFood : Node2D
{
    [Node("DetectableComponent")] private DetectableComponent _detectableComponent; 
    [Node] private HealthComponent HealthComponent; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _detectableComponent.CustomCanBeDetectedBy = CanBeDetectedBy;
        HealthComponent.OnDeath += QueueFree;
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        return TaskManager.Instance.CurrentTask is FeedCatTask;
    }
}
