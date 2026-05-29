namespace NotAloneAtHome.Components.Base.Holder;

using System.Linq;
using Godot;
using NotAloneAtHome.Components.Destroyable;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Thrower;

public partial class ComponentHolder : Node2D
{
    public Node2D Root => GetParent<Node2D>();

    // all components
    public ICarriableComponent Carriable;
    public ICarrierComponent Carrier;
    public IDetectableComponent Detectable;
    public IAreaDetectorComponent AreaDetector;
    public ICastedAreaDetectorComponent CastedAreaDetector;
    public IThrowableComponent Throwable;
    public IThrowerComponent Thrower;
    public IInteractableComponent Interactable;
    public IKillable Destroyable;

    public override async void _Ready()
    {
        Carrier            = GetChildren().OfType<ICarrierComponent>().FirstOrDefault();
        Carriable          = GetChildren().OfType<ICarriableComponent>().FirstOrDefault();
        Detectable         = GetChildren().OfType<IDetectableComponent>().FirstOrDefault();
        Throwable          = GetChildren().OfType<IThrowableComponent>().FirstOrDefault();
        AreaDetector       = GetChildren().OfType<IAreaDetectorComponent>().FirstOrDefault();
        CastedAreaDetector = GetChildren().OfType<ICastedAreaDetectorComponent>().FirstOrDefault();
        Thrower            = GetChildren().OfType<IThrowerComponent>().FirstOrDefault();
        Interactable       = GetChildren().OfType<IInteractableComponent>().FirstOrDefault();
        Destroyable        = GetChildren().OfType<IKillable>().FirstOrDefault();
        
        await ToSignal(Root, Node.SignalName.Ready);

        foreach (var child in GetChildren().OfType<IComponentBase>()) 
            child.AfterReady();

        AfterHelperReady();
    }

    /// <summary>
    /// Called after all helper nodes are ready.
    /// </summary>
    public virtual void AfterHelperReady() { }
}