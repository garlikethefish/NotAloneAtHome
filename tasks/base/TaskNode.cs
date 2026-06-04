namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public abstract partial class TaskNode : ITask
{
    public SceneTree Context { get; protected set; }
    public List<ITaskStep> Steps { get; } = [];
    public ITaskStep CurrentStep { get; set; }
    public bool IsCompleted { get; }
    public string Name { get; private set; }
    public event Action OnComplete;
    public event Action<string> OnTaskNameChanged;
    public event Action<string> OnTaskStepNameChanged;
    public event Action<ITaskStep> OnTaskStepChangedToChanged;

    public TaskNode(SceneTree ctx)
    {
        Context = ctx;
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

    public void Finish()
    {
        OnFinish();
        OnComplete?.Invoke();
    }

    public abstract void OnStart();
    public abstract void OnFinish();

    private void TransitionToStep(ITaskStep newStep)
    {
        if (CurrentStep != null)
        {
            CurrentStep.End();
            CurrentStep.OnTaskStepNameChanged -= OnTaskStepNameChanged;
        }
        CurrentStep = newStep;
        CurrentStep.OnTaskStepNameChanged += OnTaskStepNameChanged;
        CurrentStep.Start();
        OnTaskStepChangedToChanged?.Invoke(newStep);
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

    public static void Log(string value)
    {
        GD.Print($"[Task | {typeof(TaskNode).Name}] {value}");
    }
}


