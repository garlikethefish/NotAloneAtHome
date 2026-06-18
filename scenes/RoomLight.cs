using Godot;
using System;

public partial class RoomLight : PointLight2D
{
    Tween tween;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        
    }

    public override void _ExitTree()
    {
        tween.Kill();
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
