using Godot;

public interface IPlayer
{
    void DoSomething();
}

public partial class NewScript : Node2D, ICarriableUser
{
    [Export] private ComponentHolder Holder { get; set; }
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void Pickup(ICarrier carrier)
    {
        Holder.Carriable.PickUpBy(carrier);
    }

    public void Drop(Vector2 landPos)
    {
        Holder.Carriable.DropAt(landPos);
    }

    public bool CanBeCarried(ICarrier carrier)
    {
        return true;
    }
}