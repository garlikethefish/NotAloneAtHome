namespace NotAloneAtHome.Tasks;

using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class TaskManager : Node
{
    [Signal] public delegate void TaskChangedEventHandler(string title, string stepName);
    [Signal] public delegate void TaskAddedEventHandler(Node task);
    [Signal] public delegate void TaskEndedEventHandler(Node task);
    [Signal] public delegate void TaskNameChangedEventHandler(string name);
    [Signal] public delegate void TaskStepNameChangedEventHandler(string name);
    readonly List<ITask> Tasks = [];
    public ITask CurrentTask;
    public static TaskManager Instance { get; private set; }
    public override void _Ready() 
    {
        Instance = this; 
    }

    public void AddTask(ITask task)
    {
        Tasks.Add(task);
        task.OnComplete += Next;
        EmitSignal(SignalName.TaskAdded, task as Node);

        if (CurrentTask == null)
        {
            StartTask(task);
        }
    }

    public void Next()
    {
        GD.Print("Called to move on to next Task!");
        EmitSignal(SignalName.TaskEnded, CurrentTask as Node);

        EndTask(CurrentTask);

        if (Tasks.TryNextItem(CurrentTask, out var next))
        {
            EmitSignal(SignalName.TaskChanged, CurrentTask as Node, next as Node);
            StartTask(next);
        }
        else
        {
            CurrentTask = null;
        }
    }

    void StartTask(ITask task)
    {
        CurrentTask = task;
        CurrentTask.OnTaskNameChanged += OnTaskNameChanged;
        CurrentTask.OnTaskStepNameChanged += OnTaskStepNameChanged;
        CurrentTask.Start();
    }

    void EndTask(ITask task)
    {
        task.OnComplete -= Next;
        task.OnTaskNameChanged -= OnTaskNameChanged;
        task.OnTaskStepNameChanged -= OnTaskStepNameChanged;
    }

    void OnTaskNameChanged(string name)
    {
        EmitSignal(SignalName.TaskNameChanged, name);
        GD.Print("Task name => ", name);
    }

    void OnTaskStepNameChanged(string name)
    {
        EmitSignal(SignalName.TaskStepNameChanged, name);
        GD.Print("Task step name => ", name);
    }
}