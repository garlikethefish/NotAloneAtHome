namespace NotAloneAtHome.Tasks;

using System.Collections.Generic;
using Godot;
using NotAloneAtHome.Tasks.Interfaces;

public partial class TaskManager : Node
{
    [Signal] public delegate void TaskChangedEventHandler(Node previous, Node next);
    [Signal] public delegate void TaskAddedEventHandler(Node task);
    List<ITask> Tasks;
    ITask CurrentTask;
    public static TaskManager Instance { get; private set; }
    public override void _Ready() => Instance = this;

    public void AddTask(ITask task)
    {
        Tasks.Add(task);
        task.OnComplete += Next;
        EmitSignal(SignalName.TaskAdded, task as Node);

        if (CurrentTask == null)
        {
            CurrentTask = task;
            CurrentTask.Start();
        }
    }

    public void Next()
    {
        CurrentTask.OnComplete -= Next;
        if (Tasks.TryNextItem(CurrentTask, out var next))
        {
            EmitSignal(SignalName.TaskChanged, CurrentTask as Node, next as Node);
            CurrentTask = next;
            CurrentTask.Start();
        }
        else
        {
            CurrentTask = null;
        }
    }
}