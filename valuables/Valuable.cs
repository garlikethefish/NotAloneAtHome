namespace NotAloneAtHome.Valuables;

using Godot;
using NotAloneAtHome.Components;

public partial class Valuable : RigidBody2D, IInteractable, ICarriable, IThrowable, IDetectable
{
    [Export] public ValuableType Type;
    [Export] public Sprite2D Sprite;
    public ComponentHolder Holder { get; private set; }
    public IDetectableComponent DetectableComp { get; set; }
    private ShaderMaterial _shaderMat = GD.Load<ShaderMaterial>("uid://cnuuc1ep5p6ia");
    public IDetectableComponent DetectableComponent { get; set; }
    public ICarriableComponent CarriableComponent { get; set; }
    public IThrowableComponent ThrowableComponent { get; set; }
    public IInteractableComponent interactableComponent { get; set; }

    public override void _Ready()
    {
        Holder              = this.TryGetComponent<ComponentHolder>();
        DetectableComponent = Holder.DetectableComp;
        CarriableComponent  = Holder.CarriableComp;
        ThrowableComponent  = Holder.ThrowableComp;
        interactableComponent = Holder.InteractableComp;

        Sprite.Texture = ValuableData.Valuables[Type].texture2D;
        Sprite.Material = _shaderMat;

        ThrowableComponent.OnThrownBy   += ThrownBy;
        CarriableComponent.OnPickedUpBy += PickedUpBy;
        CarriableComponent.OnDropedAt   += DropedAt;
    }

    public void Sell(Node2D node)
    {
        GD.Print("Sold!");
    }

    void ThrownBy(IThrower thrower, Vector2 pos)
    {
        
    }

    void PickedUpBy(ICarrier carrier)
    {
    }

    void DropedAt(Vector2 pos)
    {
    }

    public bool CanBeCarried(ICarrier carrier)=>true;

    public void InteractedBy(IInteractor interactor)
    {
        
    }

    public void ExitAllDetectors()
    {
        DetectableComp.HandleExitAllDetectors();
    }

    public bool CanBeDetected(IAreaDetector detector)=>true;
}