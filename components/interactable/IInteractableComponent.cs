

using System;
using Godot;

public interface IInteractableComponent 
{
    void HandleInteractedBy(IInteractor interactor);
}