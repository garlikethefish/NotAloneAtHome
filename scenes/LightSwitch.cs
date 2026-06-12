using Godot;
using Godot.Collections;
using NotAloneAtHome.Components;
using System;

[Scene]
public partial class LightSwitch : Node2D
{
    [Export] Room room;
    [Node("InteractableComponent")] InteractableComponent _interactable;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        _interactable.OnInteractionFrom += ToggleLight;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    void ToggleLight(InteractorComponent interactor)
    {
        room.ToggleLight();
    }
}
