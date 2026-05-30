using Godot;

public partial class SellZone : Node2D
{
    private bool _hasAppeared = true;
    private Tween _tween;
    private Vector2 _startingScale;
    private Vector2 _startingPos;
    [Export] public Vector2 ExpandToPos { get; set; } = Vector2.Zero;
    public override void _Ready()
    {
        _startingScale = Scale;
        _startingPos = GlobalPosition;
        Disappear();
    }

    private void OnArea2DAreaEntered(Area2D area)
    {
        // var itemCost = GameManager.Valuables[valuable.Type].Value;
        // GameManager.StolenStuffAmount += 1;
        // GameManager.MoneyLost -= itemCost;

        // valuable.Sell(this);

        // GameManager.OnItemSteal?.Invoke();
        // GameManager.Suspicion = Mathf.Clamp(GameManager.Suspicion - 10, 0, 100);

        // if (GameManager.StolenStuffAmount >= GameManager.MaxStealableItems)
        //     GameManager.OnMaxItemsStolen?.Invoke();
    }

    public void Appear()
    {
        if (_hasAppeared) return;
        _hasAppeared = true;

        _tween?.Kill();
        _tween = CreateTween();
        _tween.SetParallel();
        _tween.TweenProperty(this, "scale", _startingScale, 0.5f)
              .SetTrans(Tween.TransitionType.Cubic);
        _tween.TweenProperty(this, "global_position", _startingPos + ExpandToPos, 0.5f)
              .SetTrans(Tween.TransitionType.Cubic);
    }

    public void Disappear()
    {
        if (!_hasAppeared) return;
        _hasAppeared = false;

        _tween?.Kill();
        _tween = CreateTween();
        _tween.SetParallel();
        _tween.TweenProperty(this, "scale", Vector2.Zero, 0.5f)
              .SetTrans(Tween.TransitionType.Cubic);
        _tween.TweenProperty(this, "global_position", _startingPos, 0.5f)
              .SetTrans(Tween.TransitionType.Cubic);
    }
}