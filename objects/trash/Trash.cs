using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Destroyable;
using NotAloneAtHome.Components.Detectable;
using System;
using System.Collections.Generic;

public partial class Trash : Node2D, IKillable, IInteractable, IDetectable
{
    [Export] public Sprite2D sprite2D;
    public int Health { get; private set; } = 2;

    public ComponentHolder Holder { get; private set; }
    private IDetectableComponent _detectable;
    private IInteractableComponent _interactable;
    public event Action<IAreaDetector> OnDetectableBecamePriority;
    public event Action<IAreaDetector> OnDetectableLostPriority;
    public List<IAreaDetector> BlacklistedDetectors => _detectable.BlacklistedDetectors;
    public CollisionShape2D CollisionShape2D => _detectable.CollisionShape2D;
    public Rid Rid => _detectable.HandleGetRid();

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        if (Holder == null) GD.PushError("Didnt get holder in trash.cs");
        _detectable = Holder.Detectable;
        _interactable = Holder.Interactable;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        return true;
    }

    public void WhenEnteredDetectorArea(IAreaDetector detector)
    {
        
    }

    public void WhenExitedDetectorArea(IAreaDetector detector)
    {
        
    }

    public void WhenSetAsDetectorPriority(IAreaDetector detector)
    {
        OnDetectableBecamePriority?.Invoke(detector);
    }

    public void WhenRemovedFromDetectorPriority(IAreaDetector detector)
    {
        OnDetectableLostPriority?.Invoke(detector);
    }

    public void AddToDetectorBlacklist(IAreaDetector detector)
    {
        _detectable.HandleAddToBlacklist(detector);
    }

    public void RemoveFromDetectorBlacklist(IAreaDetector detector)
    {
        _detectable.HandleRemoveFromBlacklist(detector);
    }

    public bool IsDetectorBlacklisted(IAreaDetector detector)
    {
        return _detectable.HandleIsDetectorBlacklisted(detector);
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
    }

    public void ExitAllDetectors()
    {
        _detectable.HandleExitAllDetectors();
    }
}
