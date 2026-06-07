namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using Godot;

public interface ITask
{
    /// <summary>
    /// Name of current task
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Context to scene where task should look for all its dependencies
    /// </summary>
    public SceneTree Context { get; }
    /// <summary>
    /// All steps in this task
    /// </summary>
    public List<ITaskStep> Steps { get; }
    /// <summary>
    /// Tasks current step
    /// </summary>
    public ITaskStep CurrentStep { get; }
    /// <summary>
    /// Flag that its completed 
    /// </summary>
    public bool IsCompleted { get; }
    /// <summary>
    /// Triggered when task was completed
    /// </summary>
    public event Action OnComplete;
    /// <summary>
    /// Triggered when tasks name was changed / updated
    /// </summary>
    public event Action<string> OnTaskNameChanged;
    /// <summary>
    /// Triggered when tasks step name was changed / updated
    /// </summary>
    public event Action<string> OnTaskStepNameChanged;
    /// <summary>
    /// Triggered when tasks step is changed
    /// </summary>
    public event Action<ITaskStep> OnTaskStepChangeTo;
    /// <summary>
    /// Starts current task
    /// </summary>
    public void Start();
    /// <summary>
    /// Goes to next step
    /// </summary>
    public void StepNext();
    /// <summary>
    /// Goes back a step
    /// </summary>
    public void StepBack();
    /// <summary>
    /// Goes to specific step
    /// </summary>
    public void GoToStep(ITaskStep step);

}