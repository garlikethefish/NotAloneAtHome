namespace NotAloneAtHome.Components.Base.Holder;

using System.Linq;
using Godot;
using NotAloneAtHome.Components.Destroyable;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Spawner;
using NotAloneAtHome.Components.Thrower;

public partial class ComponentHolder : Node2D
{
    public Node2D Root => GetParent<Node2D>();

    // all components
    public ICarriableComponent CarriableComp;
    public ICarrierComponent CarrierComp;
    public IDetectableComponent DetectableComp;
    public IAreaDetectorComponent AreaDetectorComp;
    public ICastedAreaDetectorComponent CastedAreaDetectorComp;
    public IThrowableComponent ThrowableComp;
    public IThrowerComponent ThrowerComp;
    public IInteractableComponent InteractableComp;
    public IKillable DestroyableComp;
    public ISpawnerComponent SpawnerComp;

    public override async void _Ready()
    {
        CarrierComp            = GetChildren().OfType<ICarrierComponent>().FirstOrDefault();
        CarriableComp          = GetChildren().OfType<ICarriableComponent>().FirstOrDefault();
        DetectableComp         = GetChildren().OfType<IDetectableComponent>().FirstOrDefault();
        ThrowableComp          = GetChildren().OfType<IThrowableComponent>().FirstOrDefault();
        AreaDetectorComp       = GetChildren().OfType<IAreaDetectorComponent>().FirstOrDefault();
        CastedAreaDetectorComp = GetChildren().OfType<ICastedAreaDetectorComponent>().FirstOrDefault();
        ThrowerComp            = GetChildren().OfType<IThrowerComponent>().FirstOrDefault();
        InteractableComp       = GetChildren().OfType<IInteractableComponent>().FirstOrDefault();
        DestroyableComp        = GetChildren().OfType<IKillable>().FirstOrDefault();
        SpawnerComp            = GetChildren().OfType<ISpawnerComponent>().FirstOrDefault();
        
        
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