extends Node
class_name ICariable

signal on_pick_up(_carrier: ICarrier)
signal on_drop(_carrier: ICarrier)

var carrier: ICarrier
var tween: Tween
@export var sprite: Sprite2D

func pick_up(_carrier: ICarrier):
	if tween:
		tween.kill()
		
	tween = create_tween()
		
	if sprite:
		tween.tween_property(sprite, "modulate:a", .5, .1)
		
	carrier = _carrier
	on_pick_up.emit(carrier)
	
func drop():
	on_drop.emit(carrier)
	
	if tween:
		tween.kill()
		
	tween = create_tween()
	tween.set_parallel()
	
	if sprite:
		tween.tween_property(sprite, "modulate:a", 1, .1)
	tween.tween_property(get_parent(), "global_position", get_parent().global_position + carrier.facingDirection * 10, .5).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	carrier = null

func can_be_carried(_carrier: ICarrier) -> bool:
	var parent := get_parent()
	return parent != null and parent.has_method("can_be_carried") and parent.can_be_carried(_carrier)
	
