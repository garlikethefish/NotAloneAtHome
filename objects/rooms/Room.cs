using Godot;
using Godot.Collections;
using NotAloneAtHome.Characters.Player;
using System;
using System.Linq;

[Scene][Tool]
public partial class Room : Node2D
{
    [Export] Array<Polygon2D> _lightPolygons = [];
    [Export] Node _lightWorld;
    [Export] Node _darkWorld;
    [Export] bool _darkenRoomIfPlayerExits = true;
    [Node("Area2D")] private Area2D _roomArea;
    private Dictionary<GodotObject, Polygon2D> _mirrors = [];
    [Export] private CollisionPolygon2D _areasCollisionPoly;
    private Polygon2D _areaMirror;
    
    private Tween _currentTween;
    public bool IsLightOn;
    public event Action OnRoomUpdated;
    public event Action<Node2D> OnBodyEntered;
    public event Action<Node2D> OnBodyExited;
    
    
	public override void _Ready()
    {
        foreach (var poly in _lightPolygons)
        {
            AddMirror(poly);
        }
        
        _areaMirror = new Polygon2D();
        var points = GetPolygonPoints(_areasCollisionPoly)
            .Select(p => GetPolygonTransform(_areasCollisionPoly) * p)
            .ToArray();
        _areaMirror.Polygon = points;
        _darkWorld.AddChild(_areaMirror);

        TweenMirrorsToAlpha(0);
        IsLightOn = false;
        TweenAreaToAlpha(1);

        _roomArea.BodyEntered += HandleBodyEntered;
        _roomArea.BodyExited += HandleBodyExited;
    }

    public override void _ExitTree()
    {
        if (!IsInstanceValid(_roomArea) || Engine.IsEditorHint()) return;
        _roomArea.BodyEntered -= HandleBodyEntered;
        _roomArea.BodyExited -= HandleBodyExited;
    }

    void HandleBodyExited(Node2D body)
    {
        if (body is Player)
        {
            TweenAreaToAlpha(1);
        }
        OnBodyExited?.Invoke(body);
    }

    void HandleBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            TweenAreaToAlpha(0);
        }
        OnBodyEntered?.Invoke(body);
    }

    private void AddMirror(Node2D source)
    {
        var mirror = new Polygon2D();
        mirror.Position = Vector2.Zero;
        var points = GetPolygonPoints(source)
            .Select(p => GetPolygonTransform(source) * p)
            .ToArray();
        mirror.Polygon = points;
        _lightWorld.AddChild(mirror);
        _mirrors[source] = mirror;
    }

    private Vector2[] GetPolygonPoints(Node2D node) => node switch
    {
        Polygon2D p => p.Polygon,
        CollisionPolygon2D p => p.Polygon,
        _ => []
    };

    private Transform2D GetPolygonTransform(Node2D node) => node switch
    {
        Polygon2D p => p.GlobalTransform,
        CollisionPolygon2D p => p.GlobalTransform,
        _ => Transform2D.Identity
    };

    public override void _Process(double delta)
    {
        foreach (var (source, mirror) in _mirrors)
        {
            if (source is not RaycastedPolygon2D raycasted) continue;
            var xform = raycasted.GlobalTransform;
            mirror.Polygon = raycasted.Polygon
                .Select(p => xform * p)
                .ToArray();
        }
    }

    public void ToggleLight()
    {
        if (IsLightOn) 
        {
            TweenMirrorsToAlpha(0); 
        } 
        else 
        {
            TweenMirrorsToAlpha(1);
        }  
        IsLightOn = !IsLightOn;
    }

	private Tween _lightTween;
    private Tween _areaTween;

    private void TweenMirrorsToAlpha(float alpha)
    {
        if (IsInstanceValid(_lightTween) && _lightTween.IsRunning())
            _lightTween.Stop();

        _lightTween = CreateTween();
        _lightTween.SetParallel();
        foreach (var mirror in _mirrors.Values)
        {
            _lightTween.TweenProperty(mirror, "modulate:a", alpha, 0.3f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
        OnRoomUpdated?.Invoke();
    }

    private void TweenAreaToAlpha(float alpha)
    {
        if (IsInstanceValid(_areaTween) && _areaTween.IsRunning())
            _areaTween.Stop();

        _areaTween = CreateTween();
        _areaTween.TweenProperty(_areaMirror, "modulate:a", alpha, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();
        
        if (!GetChildren().OfType<Area2D>().Any())
            warnings.Add("Requires a child Area2D node.");
        
        return warnings.ToArray();
    }

	public override void _ValidateProperty(Dictionary property)
	{
		UpdateConfigurationWarnings();
	}
}
