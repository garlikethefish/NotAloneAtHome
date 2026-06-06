namespace NotAloneAtHome.Tasks;

using System;
using Godot;

public interface ITaskStep
{
    public string Name { get; }
    public SceneTree Ctx { get; }
    event Action<string> OnTaskStepNameChanged;

    /// <summary>
    /// Finishes step
    /// </summary>
    public void Finish();
    /// <summary>
    /// Triggered on step's lifecicle start
    /// </summary>
    public void Start();
    /// <summary>
    /// Triggered on step's lifecicle end
    /// </summary>
    public void StepEnd();
    public void TaskEnd();
    /// <summary>
    /// Signals to go to the next step
    /// </summary>
    public void GoStepForward();
    /// <summary>
    /// Signals to go back a step. Usualy on failure
    /// </summary>
    public void GoStepBack();
}

public interface ITaskStep<T> : ITaskStep where T : ITask
{
    public T Task { get; }
}
