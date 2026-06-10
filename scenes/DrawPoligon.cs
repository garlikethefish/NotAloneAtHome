using Godot;

public partial class DrawPoligon : Polygon2D
{
    [Export] private Polygon2D _source;

    public override void _Process(double delta)
    {
        if (_source == null) return;
        if (_source.Polygon.Length < 3) return;
        GlobalTransform = _source.GlobalTransform;
        Polygon = _source.Polygon;
    }
}