using Godot;

public interface IDetectable : IComponentInterface
{
    public void EnterArea(AreaDetectorBase detector);
    public void ExitArea(AreaDetectorBase detector);
    public void SetAsAreaPriority(AreaDetectorBase detector);
    public void RemoveAsAreaPriority(AreaDetectorBase detector);
    public bool CanBeDetected(AreaDetectorBase detector);
}
