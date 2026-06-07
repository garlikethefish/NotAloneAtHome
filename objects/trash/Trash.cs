namespace NotAloneAtHome.Objects;

using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;

[Scene]
public partial class Trash : Node2D
{
    [Export] public Sprite2D sprite2D;
    [Node] public DetectableComponent DetectableComponent;
    [Node] public InteractableComponent InteractableComponent;
    [Node] public HealthComponent HealthComponent;

    public override void _Ready()
    {
        GD.Print("Detectable: ", DetectableComponent);
        DetectableComponent.CustomCanBeDetectedBy = CanBeDetectedBy;
        InteractableComponent.OnInteractionFrom += OnInteractionFrom;
        HealthComponent.OnDeath += OnDeath;

        HealthComponent.Health = 2;
    }

    void OnInteractionFrom(InteractorComponent interactorComponent)
    {
        HealthComponent.TakeDamage(1);
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        return TaskManager.Instance.CurrentTask is CollectTrashTask;
    }

    public void OnDeath()
    {
        QueueFree();
    }
}
