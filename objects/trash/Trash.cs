using Godot;
using NotAloneAtHome.Components;

public partial class Trash : Node2D, IKillable, IInteractable, IDetectable
{
    [Export] public Sprite2D sprite2D;
    public int Health { get; private set; } = 2;
    public ComponentHolder Holder { get; private set; }
    public IDetectableComponent DetectableComponent { get; set; }

    public IInteractableComponent interactableComponent => throw new System.NotImplementedException();


    public override void _Ready()
    {
        Holder = this.TryGetComponent<ComponentHolder>();
        if (Holder == null) GD.PushError("Didnt get holder in trash.cs");
        DetectableComponent = Holder.DetectableComp;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) Die();
    }

    public void Die()
    {
        GD.Print("I Died");
        QueueFree();
    }

    public void InteractedBy(IInteractor interactor)
    {
        TakeDamage(1);
    }
    public bool CanBeDetected(IAreaDetector detector)=>true;
    public void ExitAllDetectors()
    {
        DetectableComponent.HandleExitAllDetectors();
    }
}
