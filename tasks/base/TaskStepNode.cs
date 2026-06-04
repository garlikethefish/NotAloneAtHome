namespace NotAloneAtHome.Tasks;

using System;
using System.Linq;
using Godot;
using NotAloneAtHome.Components;

public abstract class TaskStepBase : ITaskStep
{
    public string Name { get; private set; }
    public SceneTree Ctx => Task.Context;
    public ITask Task { get; }
    public event Action<string> OnTaskStepNameChanged;

    public TaskStepBase(ITask task)
    {
        Task = task;
    }

    public void GoStepForward()
    {
        Task.StepNext();
    }

    public void GoStepBack()
    {
        Task.StepBack();
    }

    public void Start()
    {
        OnStart();
    }

    public void End()
    {
        UpdateName("");
        OnEnd();
    }

    public abstract void OnStart();
    public abstract void OnEnd();

    public void UpdateName(string name)
    {
        Name = name;
        OnTaskStepNameChanged?.Invoke(name);
    }
 
    public void Finish()
    {
        End();
        GoStepForward();
    }
}
