using Godot;

using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Destroyable;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;
using System;
using System.Collections.Generic;

public partial class Trash : Node2D, IKillable, IInteractable, IDetectable
{
    [Export] public Sprite2D sprite2D;
    public int Health { get; private set; } = 2;
    public ComponentHolder Holder { get; private set; }
    private IInteractableComponent _interactable;
    public ReactiveList<IAreaDetector> BlacklistedDetectors => DetectableComp.BlacklistedDetectors;
    public CollisionShape2D CollisionShape2D => DetectableComp.CollisionShape2D;
    public Rid Rid => DetectableComp.HandleGetRid();
    public Action<IAreaDetector> OnEnteredDetectorArea { get; set; }
    public Action<IAreaDetector> OnExitedDetectorArea { get; set; }
    public Action<IAreaDetector> OnBecameDetectorPriority { get; set; }
    public Action<IAreaDetector> OnRemovedDetectorPriority { get; set; }
    public IDetectableComponent DetectableComp { get; set; }

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        if (Holder == null) GD.PushError("Didnt get holder in trash.cs");
        DetectableComp = Holder.DetectableComp;
        _interactable = Holder.InteractableComp;
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
        _interactable.HandleInteractedBy(interactor);
    }
    public bool CanBeDetected(IAreaDetector detector)=>true;
    public void ExitAllDetectors()
    {
        DetectableComp.HandleExitAllDetectors();
    }
}
