
using System.Linq;
using Godot;

public partial class ComponentHolder : Node2D
{
    public RigidBody2D Root => GetParent<RigidBody2D>();

    // all helpers
    public ICarriable Carriable;
    public IDetectable Detectable;

    public override async void _Ready()
    {
        Carriable  = GetChildren().OfType<ICarriable>().FirstOrDefault();
        Detectable = GetChildren().OfType<IDetectable>().FirstOrDefault();

        await ToSignal(Root, Node.SignalName.Ready);

        // pull callables from root — compiler already guaranteed they exist
        // if (Carriable != null)
        //     Carriable.SetCanBeCarriedCallable(
        //         Callable.From<ICarrier>((Root as ICanBeCarried).CanBeCarried)
        //     );

        // if (Detectable != null)
        //     Detectable.SetCanBeDetectedCallable(
        //         Callable.From<IProximityAreaDetector>((Root as ICanBeDetected).CanBeDetected)
        //     );

        // WireCarriableDetectable();
        // WireThrowableDetectable();
        // WireThrowableCarriable();

        foreach (var child in GetChildren().OfType<ComponentNode2D>()) 
            child.AfterReady();

        AfterHelperReady();
    }

    /// <summary>
    /// Called after all helper nodes are ready.
    /// </summary>
    public virtual void AfterHelperReady() { }

    void WireHelpers()
    {
        
    }
}