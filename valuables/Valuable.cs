namespace NotAloneAtHome.Valuables;

using System;
using System.Collections.Generic;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;

public partial class Valuable : RigidBody2D, IInteractable, ICarriable, IThrowable, IDetectable
{
    [Export] public ValuableType Type;
    public ComponentHolder Holder { get; private set; }
    private ICarriableComponent _carriable;
    private IInteractableComponent _interactable;
    private IThrowableComponent _throwable;
    public IDetectableComponent DetectableComp { get; set; }
    private Sprite2D _sprite;
    private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");
    public CollisionShape2D CollisionShape2D => DetectableComp.CollisionShape2D;
    public ReactiveList<IAreaDetector> BlacklistedDetectors => DetectableComp.BlacklistedDetectors;
    public Rid Rid => DetectableComp.HandleGetRid();

    public Action<ICarrier> OnPickedUpBy { get; set; }
    public Action<Vector2> OnDropedAt { get; set; }
    public Action<IThrower, Vector2> OnThrownBy { get; set; }
    public Action<Vector2> OnLanded { get; set; }
    public Action<IAreaDetector> OnEnteredDetectorArea { get; set; }
    public Action<IAreaDetector> OnExitedDetectorArea { get; set; }
    public Action<IAreaDetector> OnBecameDetectorPriority { get; set; }
    public Action<IAreaDetector> OnRemovedDetectorPriority { get; set; }


    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        DetectableComp   = Holder.DetectableComp;
        _interactable = Holder.InteractableComp;
        _carriable    = Holder.CarriableComp;
        _throwable    = Holder.ThrowableComp;
        _sprite       = GetNode<Sprite2D>("Sprite2D");

        _sprite.Texture = ValuableData.Valuables[Type].texture2D;
        _sprite.Material = _shaderMat;

        OnThrownBy   += ThrownBy;
        OnPickedUpBy += PickedUpBy;
        OnDropedAt   += DropedAt;
    }

    public void Sell(Node2D node)
    {
        GD.Print("Sold!");
    }

    void ThrownBy(IThrower thrower, Vector2 pos)
    {
        _throwable.HandleThrownBy(thrower, pos);
    }

    void PickedUpBy(ICarrier carrier)
    {
        _carriable.HandlePickedUpBy(carrier);
    }

    void DropedAt(Vector2 pos)
    {
        _carriable.HandleDropedAt(pos);
    }

    public bool CanBeCarried(ICarrier carrier)=>true;

    public void InteractedBy(IInteractor interactor)
    {
        _interactable.HandleInteractedBy(interactor);
    }

    public void ExitAllDetectors()
    {
        DetectableComp.HandleExitAllDetectors();
    }

    public bool CanBeDetected(IAreaDetector detector)=>true;
}