namespace NotAloneAtHome.Components;

public interface IInteractor 
{
    bool CanInteract { get; set; }
    void InteractWith(IInteractable interactable);
}