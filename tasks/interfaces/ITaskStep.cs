namespace NotAloneAtHome.Tasks.Interfaces;

using Godot;

public interface ITaskStep
{
    public string Name { get; }
    public Node Context { get; }
    /// <summary>
    /// Finishes step
    /// </summary>
    public void Finish();
    /// <summary>
    /// Triggered on step's lifecicle start
    /// </summary>
    public void Start();
    /// <summary>
    /// Triggered on step's lifecicle end
    /// </summary>
    public void End();
    /// <summary>
    /// Signals to go to the next step
    /// </summary>
    public void EmitNext();
    /// <summary>
    /// Signals to go back a step. Usualy on failure
    /// </summary>
    public void EmitBack();
}
public interface ITaskStep<T> : ITaskStep where T : ITask
{
    public T Task { get; }
    
}
