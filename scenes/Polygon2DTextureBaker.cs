#if TOOLS
using Godot;

[Tool]
public partial class Polygon2DTextureBaker : Node2D
{
    [Export] public Polygon2D SourcePolygon { get; set; }
    [Export] public string SavePath { get; set; } = "res://textures/baked_light.png";

    [ExportToolButton("Bake to PointLight2D")]
    public Callable BakeButton => Callable.From(DoBake);

    public void DoBake()
    {
        if (SourcePolygon == null)
        {
            GD.PrintErr("[PolygonLightBaker] SourcePolygon or TargetLight not assigned.");
            return;
        }

        var points = SourcePolygon.Polygon;
        if (points.Length < 3)
        {
            GD.PrintErr("[PolygonLightBaker] Polygon has fewer than 3 points.");
            return;
        }

        var xform = SourcePolygon.GlobalTransform * GlobalTransform.Inverse();
        var transformed = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
            transformed[i] = xform * points[i];

        var min = transformed[0];
        var max = transformed[0];
        foreach (var p in transformed)
        {
            min = min.Min(p);
            max = max.Max(p);
        }

        int w = Mathf.CeilToInt(max.X - min.X);
        int h = Mathf.CeilToInt(max.Y - min.Y);

        if (w <= 0 || h <= 0)
        {
            GD.PrintErr("[PolygonLightBaker] Computed texture size is zero or negative.");
            return;
        }

        var image = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        for (int i = 0; i < transformed.Length; i++)
            transformed[i] -= min;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (Geometry2D.IsPointInPolygon(new Vector2(x, y), transformed))
                    image.SetPixel(x, y, Colors.White);

        // 1. Save the actual PNG file to disk
        var absPath = ProjectSettings.GlobalizePath(SavePath);
        image.SavePng(absPath);

        // 2. Force Godot's file system to notice and import the new PNG file
        var editorInterface = EditorInterface.Singleton;
        if (editorInterface != null)
        {
            var resourceFileSystem = editorInterface.GetResourceFilesystem();
            resourceFileSystem.Scan();
            resourceFileSystem.ScanSources(); 
        }

        // 3. Load the resource back from disk so the scene stores a small link path, 
        // instead of embedding a massive raw binary text block.
        var texture = GD.Load<Texture2D>(SavePath);
        GD.Print($"[PolygonLightBaker] Successfully baked {w}x{h} → {SavePath}");
    }
}
#endif