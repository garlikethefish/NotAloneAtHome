namespace NotAloneAtHome.state_machines.interfaces;

public interface IStateMachine
{
    IState CurrentState { get; }
    void ChangeState(IState next); 
}