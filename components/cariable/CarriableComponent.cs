namespace NotAloneAtHome.Components;

using System;
using System.Collections.Generic;
using Godot;
[Tool]
public partial class CarriableComponent : Node2D, ICarriableComponent
{
	[Export][MustAssign]
	public CollisionShape2D CollisionShape2D { get; private set; } = null;
	public Func<CarrierComponent, bool> CanBeCarriedBy { get; set; } = (_) => true;
	public event Action<CarrierComponent> OnPickedUpBy;
	public event Action<Vector2> OnDropedAt;
	private bool _isCarried = false;
	public bool IsCarried { 
		get => _isCarried; 
		set
		{
			if (GetParent() is RigidBody2D rigid)
			{
				if (value)
				{
					rigid.Freeze = true;
				}
				else
				{
					rigid.Freeze = false;
				}
			}
			_isCarried = value;
		} 
	}

	public override string[] _GetConfigurationWarnings()
	{
		if (CollisionShape2D == null)
			return [ "CollisionShape2D must be assigned" ];
		return [];
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
			UpdateConfigurationWarnings();
	}

	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		UpdateConfigurationWarnings();
	}

	private SignalAwaiter PlayDropAnimation(Vector2 landPos)
	{
		var tween  = CreateTween();
		var tweenX = CreateTween();
		var parent = GetParent<Node2D>();
		
		tween.SetParallel().SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(parent, "global_position:y", parent.GlobalPosition.Y - 20, 0.2f)
			.SetTrans(Tween.TransitionType.Quad);
		tween.TweenProperty(parent, "scale", new Vector2(0.5f, 0.5f), 0.2f)
			.SetTrans(Tween.TransitionType.Quad);
		
		tweenX.TweenProperty(parent, "global_position:x", landPos.X, 0.4f);
		
		tween.Chain();
		tween.TweenProperty(parent, "global_position:y", landPos.Y, 0.2f)
			.SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(parent, "scale", Vector2.One, 0.2f)
			.SetTrans(Tween.TransitionType.Quad);
		
		return ToSignal(tween, Tween.SignalName.Finished);
	}

	private SignalAwaiter PlayPickUpAnimation(Vector2 endPos)
	{        
		var tween  = CreateTween();
		var tweenX = CreateTween();
		var parent = GetParent<Node2D>();

		tween.SetParallel().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(parent, "scale", new Vector2(0.3f, 0.3f), 0.2f)
			.SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(parent, "position:y", parent.Position.Y - 20, 0.2f);
		
		tweenX.TweenProperty(parent, "position:x", endPos.X, 0.4f);
		
		tween.Chain();
		tween.TweenProperty(parent, "position:y", endPos.Y, 0.2f);
		tween.TweenProperty(parent, "scale", Vector2.One, 0.2f)
			.SetTrans(Tween.TransitionType.Expo);

		
		return ToSignal(tween, Tween.SignalName.Finished);
	}

	public async void HandlePickedUpBy(CarrierComponent carrier)
	{
		IsCarried = true;
		GetParent().Reparent(carrier.CarryPointNode);
		await PlayPickUpAnimation(Vector2.Zero);
		OnPickedUpBy?.Invoke(carrier);
	}

	public async void HandleDropedAt(Vector2 landPos)
	{
		IsCarried = false;
		GetParent().Reparent(GetTree().CurrentScene);
		await PlayDropAnimation(landPos);
		OnDropedAt?.Invoke(landPos);
	}
}
