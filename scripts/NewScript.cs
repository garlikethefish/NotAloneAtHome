using System.Collections.Generic;
using Godot;

public interface IPlayer
{
    void DoSomething();
}

public partial class NewScript : Node2D, ITaskItem, IInteractable
{
    public Node Node => throw new System.NotImplementedException();

    public ITask Task { get; private set; }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void Activate(ITask task)
    {
        Task = task;
    }

    public void Complete()
    {
        Task.OnTaskItemComplete(this);
    }

    public void InteractBy(IInteractor interactor)
    {
        if (Task is CollectTrashTask)
        {
            Complete();
        }
    }

    public bool CanBeInteractedBy(IInteractor interactor)
    {
        throw new System.NotImplementedException();
    }
}