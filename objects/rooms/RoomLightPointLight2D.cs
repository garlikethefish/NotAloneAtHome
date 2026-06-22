namespace NotAloneAtHome.Rooms;
using Godot;
using System;

public partial class RoomLightPointLight2D : PointLight2D, IRoomLight
{
    [Export] public bool IsDoorLight { get; private set; }= true;
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
        tween.TweenProperty(this, "energy", 1, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    public void TurnOff()
    {
        this.RefreshTween(ref tween);
        tween.TweenProperty(this, "energy", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}
