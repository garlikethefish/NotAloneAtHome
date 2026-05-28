

using System;
using Godot;

public interface IInteractableComponent 
{
    void WhenInteractBy(IInteractor interactor);
}