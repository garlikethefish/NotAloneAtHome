namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public abstract partial class TaskNode : ITask
{
    public SceneTree Ctx { get; protected set; }
    public List<ITaskStep> Steps { get; } = [];
    public ITaskStep CurrentStep { get; set; }
    public bool IsCompleted { get; }
    public string Name { get; private set; }
    public event Action OnComplete;
    public event Action<string> OnTaskNameChanged;
    public event Action<string> OnTaskStepNameChanged;
    public event Action<ITaskStep> OnTaskStepChangeTo;

    public TaskNode(SceneTree ctx)
    {
        Ctx = ctx;
        IsCompleted = false;
    }

    public void StepBack()
    {
        if (Steps.TryPreviousItem(CurrentStep, out var previous))
        {
            TransitionToStep(previous);
        }
    }

    public void StepNext()
    {
        if (Steps.TryNextItem(CurrentStep, out var next))
        {
            TransitionToStep(next);
        }
        else
        {
            Finish();
        }
    }

    public void GoToStep(ITaskStep step)
    {
        if (Steps.Contains(step))
        {
            TransitionToStep(step);
        }
    }

    public void Start()
    {
        TransitionToStep(Steps.First());
        OnStart();
    }

    public void End()
    {
        Steps.ForEach(step => step.TaskEnd());
        UpdateName("");
        OnEnd();
    }

    public void Finish()
    {
        End();
        OnFinish();
        OnComplete?.Invoke();
    }

    public abstract void OnStart();
    public abstract void OnEnd();
    public abstract void OnFinish();

    private void TransitionToStep(ITaskStep newStep)
    {
        if (CurrentStep != null)
        {
            CurrentStep.StepEnd();
            CurrentStep.OnTaskStepNameChanged -= OnTaskStepNameChanged;
        }
        CurrentStep = newStep;
        CurrentStep.OnTaskStepNameChanged += OnTaskStepNameChanged;
        CurrentStep.Start();
        OnTaskStepChangeTo?.Invoke(newStep);
    }

    public void UpdateName(string name)
    {
        Name = name;
        OnTaskNameChanged?.Invoke(name);
    }

    public void AddStep(ITaskStep step)
    {
        Steps.Add(step);
        step.OnTaskStepNameChanged += OnTaskStepNameChanged;
    }

    public void Log(string value)
    {
        GD.Print($"[Task | {GetType().Name}] {value}");
    }
}


