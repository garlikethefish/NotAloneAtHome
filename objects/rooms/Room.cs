using Godot;
using Godot.Collections;
using NotAloneAtHome.Characters.Player;
using System;
using System.Linq;

public enum Rooms {
    GoonRoom,
    LivingRoom,
    Bathroom,
    Kitchen,
    StorageRoom,
    Bedroom1,
    Bedroom2,
    Hallway,
    None,
}

[Scene][Tool]
public partial class Room : Node2D
{
    [Export] Rooms RoomName = Rooms.None;
    [Export] Node RoomOcluders;
    [Export] bool _darkenRoomIfPlayerExits = true;
    [Export] private CollisionPolygon2D _areasCollisionPoly;
    [Node("Area2D")] private Area2D _roomArea;
    Array<RoomLight> _lights = [];
    public bool IsLightOn;
    private Polygon2D _roomOcluderPoly;
    public event Action OnRoomUpdated;
    public event Action<Node2D> OnBodyEntered;
    public event Action<Node2D> OnBodyExited;

    private Tween _ocluderTween;
    
	public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        var lightContainer = GetTree().GetNodeFromGroup<RoomLightContainer>(RoomName.ToString());
        _lights = [.. lightContainer.GetChildren().OfType<RoomLight>()];

        TurnLightsOn();

        _roomOcluderPoly = new Polygon2D();
        var points = GetPolygonPoints(_areasCollisionPoly)
            .Select(p => GetPolygonTransform(_areasCollisionPoly) * p)
            .ToArray();
        _roomOcluderPoly.Polygon = points;
        _roomOcluderPoly.Color = new(0,0,0);
        RoomOcluders.AddChild(_roomOcluderPoly);

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
            EnableOcluder();
        }
        OnBodyExited?.Invoke(body);
    }

    void HandleBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            DisableOcluder();
        }
        OnBodyEntered?.Invoke(body);
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

    void EnableOcluder()
    {
        this.RefreshTween(ref _ocluderTween);
        _ocluderTween.TweenProperty(_roomOcluderPoly, "modulate:a", 1, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    void DisableOcluder()
    {
        this.RefreshTween(ref _ocluderTween);
        _ocluderTween.TweenProperty(_roomOcluderPoly, "modulate:a", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    public void TurnLightsOn()
    {
        IsLightOn = true;
        foreach (var light in _lights)
        {
            light.TurnOn();
        }
    }

    public void TurnLightsOff()
    {
        IsLightOn = false;
        foreach (var light in _lights)
        {
            light.TurnOff();
        }
    }

    public void ToggleLight()
    {
        if (IsLightOn) TurnLightsOff();
        else TurnLightsOn();
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
        if (!Engine.IsEditorHint()) return;

		UpdateConfigurationWarnings();
	}
}
