using System.Linq;
using Godot;
using Godot.Collections;
public partial class MergedPolygon2D : Polygon2D
{
    [Export] private Array<Polygon2D> _sources = [];

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Rebuild();
    }

    public void AddSource(Polygon2D source)
    {
        _sources = [.._sources, source];
        Rebuild();
    }

    public void AddSources(Array<Polygon2D> sources)
    {
        _sources = [.._sources, ..sources];
        Rebuild(); // only once
    }

    private void Rebuild()
    {
        var result = new Vector2[0];
        foreach (var source in _sources)
        {
            if (source == null)
            {
                continue;
            }
            if (source.Polygon.Length == 0)
            {
                continue;
            }

            var xform = source.GlobalTransform;
            var global = source.Polygon.Select(p => xform * p).ToArray();

            if (result.Length == 0)
            {
                result = global;
                continue;
            }
            var merged = Geometry2D.MergePolygons(result, global);
            if (merged.Count > 0)
                result = merged[0];
        }
        Polygon = result;
    }

    private Vector2[] LocalToGlobal(Polygon2D source)
    {
        var xform = source.GlobalTransform;
        var result = new Vector2[source.Polygon.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = xform * source.Polygon[i];
        return result;
    }

    private Vector2[] GlobalToLocal(Vector2[] points)
    {
        var xform = GlobalTransform.Inverse();
        var result = new Vector2[points.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = xform * points[i];
        return result;
    }
}