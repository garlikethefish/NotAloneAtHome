public class CollectTrashTask : ITask
{
    public bool IsActive { get; private set; }

    public bool IsCompleted { get; private set; }

    public void Finish()
    {
        IsCompleted = true;
        IsActive = false;
    }

    public void OnTaskItemComplete(ITaskItem taskItem)
    {
        throw new System.NotImplementedException();
    }

    public void Start()
    {
        IsActive = true;
        IsCompleted = false;
    }
}