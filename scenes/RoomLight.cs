using Godot;
using NotAloneAtHome.Characters.Player;
using System;
using System.Linq;

[Scene]
public partial class RoomLight : Area2D
{
    [Export] Polygon2D _lightPolygon;
    [Node] CollisionPolygon2D CollisionPolygon2D; 
    private Tween _currentTween;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        CollisionPolygon2D.Polygon = _lightPolygon.Polygon
            .Select(p => CollisionPolygon2D.ToLocal(_lightPolygon.ToGlobal(p)))
            .ToArray();
            
        TweenToAlpha(.5f);
        
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void TweenToAlpha(float alpha)
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.TweenProperty(_lightPolygon, "modulate:a", alpha, 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
            TweenToAlpha(1f);
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
            TweenToAlpha(0.5f);
    }
}
