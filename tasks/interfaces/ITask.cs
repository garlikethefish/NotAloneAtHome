namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using Godot;

public interface ITask
{
    public string Name { get; }
    public SceneTree Context { get; }
    public bool IsCompleted { get; }
    public event Action OnComplete;
    public event Action<string> OnTaskNameChanged;
    public event Action<string> OnTaskStepNameChanged;
    public event Action<ITaskStep> OnTaskStepChangedToChanged;
    public void Start();
    /// <summary>
    /// Goes to next step
    /// </summary>
    public void StepNext();
    /// <summary>
    /// Goes back a step
    /// </summary>
    public void StepBack();
    public List<ITaskStep> Steps { get; }
    public ITaskStep CurrentStep { get; }
}