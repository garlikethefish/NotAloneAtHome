using System;
using NotAloneAtHome.Components.Interactable;

public interface IInteractorComponent
{
    void HandleInteractWith(IInteractable interactable);
}