namespace NotAloneAtHome.Characters.DeadThiefCloset;

using System;
using Godot;
using NotAloneAtHome.Components.Base.Holder;
using NotAloneAtHome.Components.Detectable;
using NotAloneAtHome.Components.Interactable;

public partial class DeadBanditCloset : Node2D, IInteractable, IDetectable
{
    private ComponentHolder Holder;
    public IDetectableComponent DetectableComponent { get; set; }

    public override void _Ready()
    {
        Holder = this.GetComponentOfType<ComponentHolder>();
        DetectableComponent = Holder.DetectableComp;
    }

    public void InteractedBy(IInteractor interactor)
    {
        
    }

    public bool CanBeDetected(IAreaDetector detector)
    {
        return true;
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