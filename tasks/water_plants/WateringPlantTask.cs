namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NotAloneAtHome.Tasks.Interfaces;

public partial class WaterPlantsTask : Node, ITask
{
    public Node Context { get; }

    public List<ITaskStep> Steps { get; }

    public ITaskStep CurrentStep { get; set; }

    public bool IsCompleted { get; }

    public bool HasFilledUpWateringCan = false;

    public event Action OnComplete;

    public WaterPlantsTask(Node ctx)
    {
        Name = "Water plants";
        Context = ctx;
        IsCompleted = false;
        Steps = [
            new PickupCan(this),
            new FillUpCanStep(this),
            new WaterRedPlantStep(this),
            new WaterGreenPlantStep(this),
            new WaterBluePlantStep(this),
        ];

        // get reference to watering can
        // if can is droped then go to first pickup step like so:
        // GoToStep(Steps.First());
    }

    public void Back()
    {
        if (Steps.TryPreviousItem(CurrentStep, out var previous))
        {
            CurrentStep.End();
            EmitStepFailed(CurrentStep);
            CurrentStep = previous;
            CurrentStep.Start();   
        }
    }

    public void Next()
    {
        if (Steps.TryNextItem(CurrentStep, out var next))
        {
            CurrentStep.End();
            EmitStepComplete(CurrentStep);
            EmitStepChanged(CurrentStep);
            CurrentStep = next;
            CurrentStep.Start();
        }
        else
        {
            EmitComplete();
        }
    }

    public void GoToStep(ITaskStep step)
    {
        if (Steps.Contains(step))
        {
            CurrentStep.End();
            EmitStepFailed(CurrentStep);
            CurrentStep = step;
            CurrentStep.Start();
        }
    }

    public void Start()
    {
        CurrentStep = Steps.First();
        CurrentStep.Start();
        EmitStepChanged(CurrentStep);
    }

    public void EmitComplete()
    {
        OnComplete?.InvokeOrLog();
    }

    public void EmitStepComplete(ITaskStep task) {}

    public void EmitStepFailed(ITaskStep task) {}

    public void EmitStepChanged(ITaskStep task) {}
}


