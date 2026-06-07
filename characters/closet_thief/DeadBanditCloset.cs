namespace NotAloneAtHome.Characters.DeadThiefCloset;

using System;
using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;

[Scene]
public partial class DeadBanditCloset : Node2D
{
    [Node] public DetectableComponent DetectableComponent;

    public override void _Ready()
    {
        base._Ready();
        DetectableComponent.CustomCanBeDetectedBy = CanBeDetectedBy;
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        return TaskManager.Instance.CurrentTask is HideDeadBanditTask;
    }
    // private void HideDeadThief(ICarrier carrier)
    // {
    //     GD.Print("Hiding dead body");
    //     var deadThief = Utils.FindParentOfType<DeadThief>(carrier.Carriable);
    //     deadThief.ShowIntoCloset(this);
    //     carrier.TryToDrop();
    //     GameManager.CompleteObjective(ObjectiveModel.Objective.HideThief);
    // }

    // private bool CanBeInteractedWith(IInteractor interactor)
    // {
    //     var carrier = interactor.GetFellowHelper<ICarrier>();

    //     if (carrier?.Carriable?.MainParent is DeadThief)
    //         return true;

    //     return false;
    // }

    // private bool CanBeDetected(IProximityAreaDetector detector)
    // {
    //     var carrier = detector.GetFellowHelper<ICarrier>();

    //     if (carrier?.Carriable?.MainParent is DeadThief)
    //         return true;

    //     return false;
    // }

    // private void OnIInteractableOnInteraction(IInteractor interactor)
    // {
    //     var carrier = interactor.GetFellowHelper<ICarrier>();

    //     if (carrier?.Carriable?.MainParent is DeadThief)
    //         HideDeadThief(carrier);
    // }

   
}