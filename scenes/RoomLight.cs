using Godot;
using GTweens.Easings;
using GTweens.Tweens;
using GTweensGodot.Extensions;
using NotAloneAtHome.Characters.Player;
using System;
using System.Linq;

[Scene]
public partial class RoomLight : Area2D
{
    [Export] Polygon2D _lightPolygon;
    [Node] CollisionPolygon2D CollisionPolygon2D; 
    private GTween _currentTween;
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
        _currentTween = _lightPolygon.TweenModulate(new Color(1, 1, 1, alpha), .3f)
            .SetEasing(Easing.OutSine);
        _currentTween.Play();
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
