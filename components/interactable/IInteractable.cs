using System;
using Godot;

public interface IInteractable 
{
    void InteractedBy(IInteractor interactor);
    bool CanBeInteractedBy(IInteractor interactor);
}