namespace NotAloneAtHome.Valuables;

using System;
using System.Collections.Generic;
using Godot;


public partial class Valuable : RigidBody2D, IInteractable, ICarriable, IThrowable, IDetectable
{
    [Export] public ValuableType Type;

    public ComponentHolder Holder { get; private set; }
    // private ISpawnable   _spawnable;
    private ICarriable   _carriable;
    private IInteractable _interactable;
    private IThrowable   _throwable;
    private IDetectable  _detectable;
    private Sprite2D     _sprite;

    private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");

    public event Action<IAreaDetector> OnBecamePriority;
    public event Action<IAreaDetector> OnLostPriority;

    public Node Node => this;
    public Node2D Root => this;
    [Export] public CollisionShape2D CollisionShape2D { get; private set; }
    public List<IAreaDetector> BlacklistedDetectors => _detectable.BlacklistedDetectors;

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        // _spawnable    = _componentHolder.
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
        // _destroyable.Destroy(node);
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
    }

    public bool CanBeCarried(ICarrier carrier)
    {
        return true;
    }

    // private void OnIDetectableOnBecomingPriorityOfDetector(Node areaDetector)
    // {
    //     GD.Print("ON");
    //     _interactable.ShowSprite();
    //     _sprite.SetInstanceShaderParameter("enabled", true);
    // }

    // private void OnIDetectableOnNotBeeingAPriorityOfDetector(Node areaDetector)
    // {
    //     GD.Print("off");
    //     _interactable.HideSprite();
    //     _sprite.SetInstanceShaderParameter("enabled", false);
    // }

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        return true;
    }

    public void WhenInteractBy(IInteractor interactor)
    {
        GD.Print("You Touched ME!");
    }

    public void WhenThrownBy(IThrower thrower, Vector2 toPosition)
    {
        _throwable.WhenThrownBy(thrower, toPosition);
    }

    public void WhenPickedUpBy(ICarrier carrier)
    {
        _carriable.WhenPickedUpBy(carrier);
    }

    public void WhenDropedAt(Vector2 landPos)
    {
        GD.Print("I was droped!");
        _carriable.WhenDropedAt(landPos);
    }

    public void WhenEnteredDetectorArea(IAreaDetector detector)
    {
        throw new System.NotImplementedException();
    }

    public void WhenExitedDetectorArea(IAreaDetector detector)
    {
        GD.Print("I! Type: " + Type + "Exited a detector area!");
    }

    public void WhenSetAsDetectorPriority(IAreaDetector detector)
    {
        throw new System.NotImplementedException();
    }

    public void WhenRemovedFromDetectorPriority(IAreaDetector detector)
    {
        throw new System.NotImplementedException();
    }

    public void AddToBlacklist(IAreaDetector detector)
    {
        _detectable.AddToBlacklist(detector);
    }

    public void RemoveFromBlacklist(IAreaDetector detector)
    {
        _detectable.RemoveFromBlacklist(detector);
    }

    public bool IsDetectorBlacklisted(IAreaDetector detector)
    {
        throw new System.NotImplementedException();
    }

    public void ExitAllDetectors()
    {
        throw new System.NotImplementedException();
    }
}