public interface ITask
{
    bool IsActive { get; }
    bool IsCompleted { get; }
	void Start();
    void Finish();
    void OnTaskItemComplete(ITaskItem taskItem);
}
