using Godot;

public interface IDetectable : IComponentInterface
{
    public void EnterArea(AreaDetector detector);
    public void ExitArea(AreaDetector detector);
    public void SetAsAreaPriority(AreaDetector detector);
    public void RemoveAsAreaPriority(AreaDetector detector);
    public bool CanBeDetected(AreaDetector detector);
}
