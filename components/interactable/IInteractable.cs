using System;
using Godot;

public interface IInteractable 
{
    void WhenInteractBy(IInteractor interactor);
    bool CanBeInteractedBy(IInteractor interactor);
}