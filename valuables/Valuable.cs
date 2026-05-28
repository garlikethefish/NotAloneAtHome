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
    public Rid Rid => _detectable.Rid;

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

    public void WhenInteractBy(IInteractor interactor)
    {
        
    }

    public void OnCarriablePickedUpBy(ICarrier carrier)
    {
        _carriable.OnPickedUpBy(carrier);
    }

    public void OnCarriableDropedAt(Vector2 landPos)
    {
        _carriable.OnDropedAt(landPos);
    }

    public void OnDetectableEnteredDetectorArea(IAreaDetector detector)
    {
        
    }

    public void OnDetectableExitedDetectorArea(IAreaDetector detector)
    {
        
    }

    public void OnDetectableSetAsDetectorPriority(IAreaDetector detector)
    {
        OnDetectableBecamePriority?.Invoke(detector);
    }

    public void OnDetectableRemovedFromDetectorPriority(IAreaDetector detector)
    {
        OnDetectableLostPriority?.Invoke(detector);
    }

    public void DetectableAddToBlacklist(IAreaDetector detector)
    {
        _detectable.AddToBlacklist(detector);
    }

    public void DetectableRemoveFromBlacklist(IAreaDetector detector)
    {
        _detectable.RemoveFromBlacklist(detector);
    }

    public bool DetectableIsDetectorBlacklisted(IAreaDetector detector)
    {
        return _detectable.IsDetectorBlacklisted(detector);
    }

    public void ExitAllDetectors()
    {
        _detectable.ExitAllDetectors();
    }

    public void GotThrownBy(IThrower thrower)
    {
        _throwable.HandleThrownBy(thrower, Vector2.Down);
    }

    public void GotLandedOn(Vector2 pos)
    {
        
    }
}