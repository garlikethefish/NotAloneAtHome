
using System.Linq;
using Godot;
using NotAloneAtHome.Components.Destroyable;

public partial class ComponentHolder : Node2D
{
    public Node2D Root => GetParent<Node2D>();

    // all helpers
    public ICarriable Carriable;
    public ICarrier Carrier;
    public IDetectable Detectable;
    public IAreaDetector AreaDetector;
    public IThrowable Throwable;
    public IThrower Thrower;
    public IInteractable Interactable;
    public IInteractor Interactor;
    public IDestroyable Destroyable;

    public override async void _Ready()
    {
        Carrier      = GetChildren().OfType<ICarrier>().FirstOrDefault();
        Carriable    = GetChildren().OfType<ICarriable>().FirstOrDefault();
        Detectable   = GetChildren().OfType<IDetectable>().FirstOrDefault();
        Throwable    = GetChildren().OfType<IThrowable>().FirstOrDefault();
        AreaDetector = GetChildren().OfType<IAreaDetector>().FirstOrDefault();
        Thrower      = GetChildren().OfType<IThrower>().FirstOrDefault();
        Interactable = GetChildren().OfType<IInteractable>().FirstOrDefault();
        Interactor   = GetChildren().OfType<IInteractor>().FirstOrDefault();
        Destroyable   = GetChildren().OfType<IDestroyable>().FirstOrDefault();
        
        await ToSignal(Root, Node.SignalName.Ready);

        foreach (var child in GetChildren().OfType<IComponentBase>()) 
            child.AfterReady();

        AfterHelperReady();
    }

    /// <summary>
    /// Called after all helper nodes are ready.
    /// </summary>
    public virtual void AfterHelperReady() { }

    public bool HasComp<T>(out T result) where T : class
    {
        result = GetChildren().OfType<T>().FirstOrDefault();
        return result != null;
    }

}