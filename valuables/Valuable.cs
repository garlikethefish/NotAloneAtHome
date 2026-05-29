namespace NotAloneAtHome.Valuables;

using System;
using System.Collections.Generic;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;

public partial class Valuable : RigidBody2D, IInteractable, ICarriable, IThrowable, IDetectable
{
    [Export] public ValuableType Type;

    public ComponentHolder Holder { get; private set; }
    private ICarriableComponent _carriable;
    private IInteractableComponent _interactable;
    private IThrowableComponent _throwable;
    private IDetectableComponent _detectable;
    private Sprite2D _sprite;
    private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");
    public event Action<IAreaDetector> OnDetectableBecamePriority;
    public event Action<IAreaDetector> OnDetectableLostPriority;
    public CollisionShape2D CollisionShape2D => _detectable.CollisionShape2D;
    public List<IAreaDetector> BlacklistedDetectors => _detectable.BlacklistedDetectors;
    public Rid Rid => _detectable.HandleGetRid();

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        _detectable   = Holder.Detectable;
        _interactable = Holder.Interactable;
        _carriable    = Holder.Carriable;
        _throwable    = Holder.Throwable;
        _sprite       = GetNode<Sprite2D>("Sprite2D");

        _sprite.Texture = ValuableData.Valuables[Type].texture2D;
        _sprite.Material = _shaderMat;
    }

    public void Sell(Node2D node)
    {
        GD.Print("Sold!");
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
    }

    public bool CanBeCarried(ICarrier carrier)
    {
        return true;
    }

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        return true;
    }

    public void InteractedBy(IInteractor interactor)
    {
        _interactable.HandleInteractedBy(interactor);
    }

    public void WhenPickedUpBy(ICarrier carrier)
    {
        _carriable.HandlePickedUpBy(carrier);
    }

    public void WhenDropedAt(Vector2 landPos)
    {
        _carriable.HandleDropedAt(landPos);
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

    public void ExitAllDetectors()
    {
        _detectable.HandleExitAllDetectors();
    }

    public void WhenThrownBy(IThrower thrower, Vector2 pos)
    {
        _throwable.HandleThrownBy(thrower, pos);
    }

    public void WhenLandedOn(Vector2 pos)
    {
        
    }
}