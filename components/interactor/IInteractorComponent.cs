namespace NotAloneAtHome.Components;

using System;

public interface IInteractorComponent
{
    void HandleInteractWith(IInteractable interactable);
}