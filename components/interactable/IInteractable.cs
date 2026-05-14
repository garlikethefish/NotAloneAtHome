using Godot;

public interface IInteractable : IComponentInterface
{
    void InteractBy(IInteractor interactor);
    bool CanBeInteractedBy(IInteractor interactor);
}