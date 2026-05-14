

public interface ITaskItem
{
    ITask Task { get; }
    void Activate(ITask task);
    void Complete();
}