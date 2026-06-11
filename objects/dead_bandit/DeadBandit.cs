namespace NotAloneAtHome.Objects;

using Godot;
using NotAloneAtHome.Characters.DeadThiefCloset;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;

[Scene]
public partial class DeadBandit : Node2D
{
    [Node] public DetectableComponent DetectableComponent;
    [Node] public HealthComponent HealthComponent;
    private bool _isShowingIntoCloset;

    public override void _Ready()
    {
        base._Ready();
        DetectableComponent.CustomCanBeDetectedBy = CanBeDetectedBy;
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        return TaskManager.Instance.CurrentTask is HideDeadBanditTask && !_isShowingIntoCloset;
    }

    public void PlayShoweThatFatFuckIntoGrannysClosetAnimation(DeadBanditCloset closet)
    {
        if (_isShowingIntoCloset) return;
        _isShowingIntoCloset = true;

        Reparent(GetTree().CurrentScene);

        LookAt(closet.GlobalPosition);
        var tween = CreateTween();
        tween.Finished += () => {
            if (closet.HasChild<AnimationPlayer>(out var closetAnim))
                closetAnim.Play("wiggle");
            HealthComponent.TakeDamage(10000);
        };

        tween.TweenProperty(this, "global_position", closet.GlobalPosition, .3f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
    }
}