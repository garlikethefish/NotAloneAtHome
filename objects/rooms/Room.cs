using Godot;
using Godot.Collections;
using NotAloneAtHome.Characters.Player;
using NotAloneAtHome.Rooms;
using System;
using System.Drawing;
using System.Linq;

public enum Rooms {
    GoonRoom,
    LivingRoom,
    GreenRoom,
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
    [Export] Room[] AdjecentRooms = [];
    [Export] Node RoomOcluders;
    [Export] bool _darkenRoomIfPlayerExits = true;
    [Export] private CollisionPolygon2D _areasCollisionPoly;
    [Export] private Polygon2D _roomLightPoly2D;
    [Node("Area2D")] private Area2D _roomArea;
    [Node("Lights")] private Node2D _lightContainer;
    private Polygon2D _roomOcluderPoly2D;
    IRoomLight[] _lights = [];
    public bool IsLightOn;
    public event Action OnRoomUpdated;
    public event Action<Node2D> OnBodyEntered;
    public event Action<Node2D> OnBodyExited;

    private Tween _ocluderTween;
    
	public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        _lights = [.. _lightContainer.GetChildren().OfType<IRoomLight>()];

        InitializeOcluderPoly2D(_roomLightPoly2D);
        TurnLightsOff(true, true);

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

            if (IsLightOn)
            {
                TurnOnlyPointLightsOn();
            }
        }
        OnBodyExited?.Invoke(body);
    }

    void HandleBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            DisableOcluder();
            TurnOnlyPointLightsOff();
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
        if (!IsInstanceValid(_roomOcluderPoly2D)) return;

        this.RefreshTween(ref _ocluderTween);
        _ocluderTween.TweenProperty(_roomOcluderPoly2D, "modulate:a", 1, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    void DisableOcluder()
    {
        if (!IsInstanceValid(_roomOcluderPoly2D)) return;

        this.RefreshTween(ref _ocluderTween);
        _ocluderTween.TweenProperty(_roomOcluderPoly2D, "modulate:a", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    public void TurnLightsOn(bool turnPolys, bool turnPoints)
    {
        foreach (var light in _lights)
        {
            if (turnPoints && light is PointLight2D) light.TurnOn();
            if (turnPolys && light is Polygon2D) light.TurnOn();
        }
        IsLightOn = true;
    }

    public void TurnLightsOff(bool turnPolys, bool turnPoints)
    {
        foreach (var light in _lights)
        {
            if (turnPoints && light is PointLight2D) light.TurnOff();
            if (turnPolys && light is Polygon2D) light.TurnOff();
        }
        IsLightOn = false;
    }

    public void TurnOnlyPolyLightsOn()
    {
        foreach (var light in _lights)
        {
            if (light is Polygon2D) light.TurnOn();
        }
    }

    public void TurnOnlyPolyLightsOff()
    {
        foreach (var light in _lights)
        {
            if (light is Polygon2D) light.TurnOff();
        }
    }

    public void TurnOnlyPointLightsOn()
    {
        foreach (var light in _lights)
        {
            if (light is PointLight2D) light.TurnOn();
        }
    }

    public void TurnOnlyPointLightsOff()
    {
        foreach (var light in _lights)
        {
            if (light is PointLight2D) light.TurnOff();
        }
    }

    public void ToggleLight()
    {
        var turnPointlightsOn = !RoomManager.Instance.RoomsPlayerIsIn.Contains(this);
        if (IsLightOn) TurnLightsOff(true, turnPointlightsOn);
        else TurnLightsOn(true, turnPointlightsOn);
    }

    void InitializeOcluderPoly2D(Polygon2D poly)
    {
        if (poly is null) return;

        _roomOcluderPoly2D = new Polygon2D();
        AddChild(_roomOcluderPoly2D);
        _roomOcluderPoly2D.Polygon = _roomLightPoly2D.Polygon;
        _roomOcluderPoly2D.TopLevel = true;
        _roomOcluderPoly2D.Color = new(0,0,0);
        _roomOcluderPoly2D.ZIndex = 2;
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
