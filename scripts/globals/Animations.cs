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

public struct SpringVector2
{
    public float Stiffness;
    public float Damping;

    private Vector2 _velocity;
    public Vector2 Current { get; private set; }

    public SpringVector2(float stiffness, float damping)
    {
        Stiffness = stiffness;
        Damping = damping;
        _velocity = Vector2.Zero;
        Current = Vector2.Zero;
    }

    public void Tick(Vector2 target, float delta)
    {
        if (Stiffness == 0f) { Current = target; return; }
        Vector2 force = (target - Current) * Stiffness;
        _velocity += (force - _velocity * Damping) * delta;
        Current += _velocity * delta;
    }

    public void Reset(Vector2 value)
    {
        Current = value;
        _velocity = Vector2.Zero;
    }
}