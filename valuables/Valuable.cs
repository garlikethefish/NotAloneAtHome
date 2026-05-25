namespace NotAloneAtHome.Valuables;
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

    public Node Node => this;
    public Node2D Root => this;
    [Export] public CollisionShape2D CollisionShape2D { get; private set; }

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

    public bool CanBeDetected(AreaDetectorBase detector)
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
        _carriable.WhenDropedAt(landPos);
    }

    public void EnterArea(AreaDetectorBase detector)
    {
        throw new System.NotImplementedException();
    }

    public void ExitArea(AreaDetectorBase detector)
    {
        throw new System.NotImplementedException();
    }

    public void SetAsAreaPriority(AreaDetectorBase detector)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAsAreaPriority(AreaDetectorBase detector)
    {
        throw new System.NotImplementedException();
    }
}