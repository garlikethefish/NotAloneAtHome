namespace NotAloneAtHome.state_machines.interfaces;

public interface IState
{
    void Update(double delta);
    void PhysicsUpdate(double delta);
    void Enter();
    void Exit();
}

public interface IState<T> : IState where T : IStateMachine
{
    public T Ctx { get; }
}