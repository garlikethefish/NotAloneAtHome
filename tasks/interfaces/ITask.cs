namespace NotAloneAtHome.Tasks.Interfaces;

using System;
using System.Collections.Generic;
using Godot;

public interface ITask
{
    public Node Context { get; }
    public event EventHandler OnComplete;
    public void Start();
    /// <summary>
    /// Goes to next step
    /// </summary>
    public void Next();
    /// <summary>
    /// Goes back a step
    /// </summary>
    public void Back();
    public List<ITaskStep> Steps { get; }
    public ITaskStep CurrentStep { get; }
    /// <summary>
    /// Signals that task is completed!
    /// </summary>
    public void EmitComplete();
    /// <summary>
    /// Signals that tasks step is completed!
    /// </summary>
    public void EmitStepComplete(ITaskStep task);
    /// <summary>
    /// Signals that tasks step has failed!
    /// </summary>
    public void EmitStepFailed(ITaskStep task);
    /// <summary>
    /// Signals that tasks step has failed!
    /// </summary>
    public void EmitStepChanged(ITaskStep task);
}