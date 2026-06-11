namespace NotAloneAtHome.Tasks;

using System;
using System.Linq;
using Godot;

public abstract class TaskStepBase : ITaskStep
{
    public string Name { get; private set; }
    public SceneTree Ctx => Task.Ctx;
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

    public void StepEnd()
    {
        UpdateName("");
        OnStepEnd();
    }

    public void TaskEnd()
    {
        UpdateName("");
        OnTaskEnd();
    }

    public abstract void OnStart();
    public abstract void OnStepEnd();
    public abstract void OnTaskEnd();

    public void UpdateName(string name)
    {
        Name = name;
        OnTaskStepNameChanged?.Invoke(name);
    }
 
    public void Finish()
    {
        StepEnd();
        GoStepForward();
    }

    public void Log(string value)
    {
        GD.Print($"[Task | {Task.GetType().Name} | Step | {GetType().Name}] {value}");
    }
}
