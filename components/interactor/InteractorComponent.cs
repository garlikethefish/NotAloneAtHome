using Godot;

public partial class InteractorComponent : ComponentNode2D, IInteractor
{
    [Signal] public delegate void InteractedEventHandler(GodotObject interactable);
    public void InteractWith(IInteractable interactable)
    {
        interactable.WhenInteractBy(this);
        EmitSignal(SignalName.Interacted, interactable.Node);
    }
}
