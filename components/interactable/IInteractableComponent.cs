namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IInteractableComponent
{
    event Action<IInteractor> OnInteraction;
}