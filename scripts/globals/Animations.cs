using Godot;

public partial class Animations : Node
{
    public static Animations Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void PressDown(Control control, float originalY)
    {
        var tween = control.CreateTween();
        tween.TweenProperty(control, "position:y", originalY + 5, 0.08f)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void PressUp(Control control, float originalY)
    {
        var tween = control.CreateTween();
        tween.TweenProperty(control, "position:y", originalY, 0.08f)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void Unavailable(Control control)
    {
        var tween = control.CreateTween();
        tween.TweenProperty(control, "modulate:a", 0.2f, 0.08f)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void Available(Control control)
    {
        var tween = control.CreateTween();
        tween.TweenProperty(control, "modulate:a", 1f, 0.08f)
            .SetTrans(Tween.TransitionType.Sine);
    }
}