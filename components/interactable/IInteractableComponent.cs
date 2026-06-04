namespace NotAloneAtHome.Components;

using System;
using Godot;

public interface IInteractableComponent
{
    event Action<InteractorComponent> OnInteractionFrom;
    void HandleInteraction(InteractorComponent interactor);
}