extends Helper

class_name ICarriable

signal on_pick_up(_carrier: ICarrier)
signal on_drop(_carrier: ICarrier)

var can_be_carried_callable: Callable
var carrier: ICarrier

@export var sprite: Sprite2D

func _ready():
	pass

func pick_up(_carrier: ICarrier):
	if sprite:
		get_sprite_tween().tween_property(sprite, "modulate:a", .5, .1)
		
	carrier = _carrier
	on_pick_up.emit(carrier)
	
func drop(show_animation: bool = true):
	on_drop.emit(carrier)
	if show_animation: start_drop_animation()
	if sprite:
		var sprite_tween = get_sprite_tween()
		sprite_tween.tween_property(sprite, "modulate:a", 1, .1)
	carrier = null

func start_drop_animation():
	var position_tween = get_position_tween()
	
	var lastFacingDirection = carrier.facingDirection
	var parent = helper_holder.main_parent
	
	if sprite:
		var sprite_tween = get_sprite_tween()
		sprite_tween.tween_property(sprite, "modulate:a", 1, .1)
		
	position_tween.tween_property(parent, "global_position", parent.global_position + lastFacingDirection * 10, .5).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)

func can_be_carried(_carrier: ICarrier) -> bool:
	return can_be_carried_callable.call(_carrier)

func assert_assigned_callables() -> void:
	assert_callable(can_be_carried_callable, "can_be_carried_callable")
