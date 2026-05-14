public abstract class TaskStepBase : ITaskStep
{

    public bool IsComplete => throw new System.NotImplementedException();

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