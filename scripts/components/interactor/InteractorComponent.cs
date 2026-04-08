using Godot;

public partial class InteractorComponent : ComponentNode2D, IInteractor
{
    [Signal] public delegate void InteractedEventHandler(GodotObject interactable);
    public Node Node => this;

    public void InteractWith(IInteractable interactable)
    {
        if (!CanInteractWith(interactable)) return;
        
        interactable.InteractBy(this);
        EmitSignal(SignalName.Interacted, interactable.Node);
    }

    public bool CanInteractWith(IInteractable interactable)
    {
        return (Root as IInteractor)?.CanInteractWith(interactable) ?? false;
    }
}
