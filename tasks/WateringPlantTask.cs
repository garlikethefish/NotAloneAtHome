using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class TaskGroups
{
    public const string CollectTrash = "CollectTrashTask";
    public const string WaterPlant   = "WaterPlantTask";
}

public partial class WateringCan : Node2D
{
    
}
public class WaterPlantTask : IStepTask
{
    public ITaskStep FirstStep { get; private set; }
    public bool IsCompleted { get; private set; }
    public Action<IStepTask> OnCompleted { get; private set; }

    public ITaskStep CurrentStep => _current?.Value;

    private LinkedList<ITaskStep>     _steps   = new();
    private LinkedListNode<ITaskStep> _current;
    private bool _isCanFilled = false;

    Carriable wateringCanCarriable;
    private SceneTree _tree;

    public WaterPlantTask(SceneTree tree) => _tree = tree;

    public LinkedListNode<ITaskStep> pickupTask;
    public LinkedListNode<ITaskStep> fillUpCanTask;
    public LinkedListNode<ITaskStep> waterPlantTask;

    public void Start()
    {
        pickupTask     = _steps.AddLast(new PickupCanTaskStep());
        fillUpCanTask  = _steps.AddLast(new FillUpWateringCanTaskStep());
        waterPlantTask = _steps.AddLast(new WatterPlantTaskStep());

        fillUpCanTask.Value.OnCompleted += () => _isCanFilled = true;

        _current = _steps.First;
        _current.Value.Start();
        _current.Value.OnCompleted += GoToNextStep;

        var can = _tree.GetNodesInGroup(TaskGroups.WaterPlant)
            .OfType<WateringCan>()
            .FirstOrDefault();

        wateringCanCarriable = (Carriable)can.GetNode<ComponentHolder>("ComponentHolder").Carriable.Node;
        wateringCanCarriable.PickedUp += OnPickedUp;
        wateringCanCarriable.Dropped  += OnCanDropped;
    }

    void GoToNextStep()
    {
        _current.Value.OnCompleted -= GoToNextStep;
        var next = _current.Next;

        if (next == null) { 
            // No more steps
            IsCompleted = true; 
            OnCompleted?.Invoke(this);
            return;
        }

        if (next == fillUpCanTask && _isCanFilled)
        {
            // skip next
            next = next.Next;
        }

        _current = next;
        _current.Value.OnCompleted += GoToNextStep;
    }

    void GoBackTo<T>() where T : ITaskStep
    {
        var node = _current;
        while (node != null)
        {
            if (node.Value is T)
            {
                _current.Value.Stop();
                _current.Value.OnCompleted -= GoToNextStep;
                _current = node;
                _current.Value.Start();
                _current.Value.OnCompleted += GoToNextStep;
                return;
            }
            node = node.Previous;
        }
    }

    public void Finish()
    {
        wateringCanCarriable.PickedUp -= OnPickedUp;
        wateringCanCarriable.Dropped  -= OnCanDropped;
    }

    private void OnPickedUp(Node2D carrier)
    {
        if (CurrentStep is PickupCanTaskStep)
        {
            _current.Value.Complete();
        }
    }

    public void OnCanDropped()
    {
        if (CurrentStep is FillUpWateringCanTaskStep)
        {
            GoBackTo<PickupCanTaskStep>();
        }
    }

    public void RegressionActions()
    {
        throw new NotImplementedException();
    }
}

public class PickupCanTaskStep : ITaskStep
{
    public bool IsComplete { get; private set; }
    public event Action OnCompleted;

    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Complete()
    {
        OnCompleted?.Invoke();
    }
}

public class FillUpWateringCanTaskStep : ITaskStep
{
    public bool IsComplete { get; private set; }

    public event Action OnCompleted;
    public event Action<ITaskStep> OnRegressed;

    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Complete()
    {
        throw new System.NotImplementedException();
    }
}

public class WatterPlantTaskStep : ITaskStep
{
    public bool IsComplete { get; private set; }

    public event Action OnCompleted;
    public event Action<ITaskStep> OnRegressed;

    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Complete()
    {
        throw new System.NotImplementedException();
    }
}

public interface ITask
{
    bool IsActive { get; }
    bool IsCompleted { get; }
	void Start();
    void Finish();
    void OnTaskItemComplete(ITaskItem taskItem);
}


public interface IStepTask
{
    bool IsCompleted { get; }
    ITaskStep FirstStep { get; }

    ITaskStep CurrentStep { get; }
    void Start();
    void Finish();
    void RegressionActions();
}

public interface ITaskStep
{
    bool IsComplete { get; }
    void Start();
    void Stop();
    void Complete();
    event Action OnCompleted;
}