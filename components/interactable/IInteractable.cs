using Godot;

public interface IInteractable : IComponentInterface
{
    void WhenInteractBy(IInteractor interactor);
    bool CanBeInteractedBy(IInteractor interactor);
}