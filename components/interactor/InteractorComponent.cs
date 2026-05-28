using Godot;

public partial class InteractorComponent : ComponentNode2D, IInteractorComponent
{
    public void InteractWith(IInteractable interactable)
    {
        interactable.WhenInteractBy((IInteractor)Root);
    }
}
