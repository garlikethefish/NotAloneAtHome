namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NotAloneAtHome.Tasks.Interfaces;

public partial class CollectTrashTask : Node, ITask
{
    public Node Context { get; }

    public List<ITaskStep> Steps { get; }

    public ITaskStep CurrentStep { get; set; }

    public bool IsCompleted { get; }

    public event Action OnComplete;
    protected int _trashToCollect;

    public CollectTrashTask(Node ctx, int trashToCollect)
    {
        Name = "Water plants";
        Context = ctx;
        IsCompleted = false;
        _trashToCollect = trashToCollect;
        Steps = [
            
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
        OnComplete?.Invoke();
    }

    public void EmitStepComplete(ITaskStep task) {}

    public void EmitStepFailed(ITaskStep task) {}

    public void EmitStepChanged(ITaskStep task) {}
}


