namespace NotAloneAtHome.Rooms;
using Godot;
using System;

public partial class RoomLightPolygon2D : Polygon2D, IRoomLight
{
    Tween tween;

    public override void _EnterTree()
    {
        AddToGroup("dynamic_lights");
    }
	
    public override void _ExitTree()
    {
        tween?.Kill();
    }

	public void TurnOn()
    {
        this.RefreshTween(ref tween);
        tween.TweenProperty(this, "modulate:a", 1, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    public void TurnOff()
    {
        this.RefreshTween(ref tween);
        tween.TweenProperty(this, "modulate:a", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}
