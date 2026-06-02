namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IInteractableComponent
{
    event Action<InteractorComponent> OnInteraction;
    void HandleInteraction(InteractorComponent interactor);
}