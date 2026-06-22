namespace NotAloneAtHome.Rooms;

public interface IRoomLight
{
    bool IsDoorLight { get; }
    void TurnOn();
    void TurnOff();
}