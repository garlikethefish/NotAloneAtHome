using Godot;
using NotAloneAtHome.Components.Destroyable;
using System;
using System.Collections.Generic;

public partial class Trash : Node2D, IDestroyable, IInteractable, IDetectable
{
    [Export] public Sprite2D sprite2D;
    public int Health { get; private set; } = 2;

    public ComponentHolder Holder { get; private set; }
    private IDetectable _detectable;
    public event Action<IAreaDetector> OnBecamePriority;
    public event Action<IAreaDetector> OnLostPriority;

    public Node Node => this;

    public Node2D Root => this;

    public List<IAreaDetector> BlacklistedDetectors => _detectable.BlacklistedDetectors;

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        if (Holder == null) GD.PushError("Didnt get holder in trash.cs");
        _detectable = Holder.Detectable;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) OnDeath();
    }

    public void OnDeath()
    {
        GD.Print("I Died");
        QueueFree();
    }

    public void WhenInteractBy(IInteractor interactor)
    {
        TakeDamage(1);
    }

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        return true;
    }

    public void WhenEnteredDetectorArea(IAreaDetector detector) {}

    public void WhenExitedDetectorArea(IAreaDetector detector) {}

    public void WhenSetAsDetectorPriority(IAreaDetector detector) {}

    public void WhenRemovedFromDetectorPriority(IAreaDetector detector) {}

    public void AddToBlacklist(IAreaDetector detector) {}

    public void RemoveFromBlacklist(IAreaDetector detector) {}

    public bool IsDetectorBlacklisted(IAreaDetector detector)
    {
        return _detectable.IsDetectorBlacklisted(detector);
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
    }

    public void ExitAllDetectors()
    {
        throw new NotImplementedException();
    }
}
