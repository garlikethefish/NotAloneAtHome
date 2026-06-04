namespace NotAloneAtHome.Objects;

using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;

[Scene]
public partial class DeadBandit : Node2D
{
    [Node] public DetectableComponent DetectableComponent;

    public override void _Ready()
    {
        base._Ready();
        DetectableComponent.CanBeDetectedBy = CanBeDetectedBy;
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        return TaskManager.Instance.CurrentTask is HideDeadBanditTask;
    }
}