extends Helper

class_name ICarriable

signal on_pick_up(_carrier: ICarrier)
signal on_drop(_carrier: ICarrier)

var can_be_carried_callable: Callable
var carrier: ICarrier
var is_being_carried := false

@export var sprite: Sprite2D
@export var cs: CollisionShape2D
@export var rb: RigidBody2D


func pick_up(_carrier: ICarrier):
	var parent = main_parent as RigidBody2D
	parent.set_deferred("freeze", true)
	parent.reparent(_carrier.carry_point, true)
	
	carrier = _carrier
	
	helper_holder.start_doing_unskippable_shit(self)
	await play_pick_up_animation(Vector2.ZERO)
	helper_holder.stop_doing_unskippable_shit(self)
	
	is_being_carried = true
	on_pick_up.emit(carrier)


func drop(land_pos: Vector2):
	var parent = main_parent as RigidBody2D
	parent.reparent(get_tree().current_scene, true)
	
	helper_holder.start_doing_unskippable_shit(self)
	await play_drop_animation(land_pos)
	helper_holder.stop_doing_unskippable_shit(self)
	
	is_being_carried = false
	
	if parent is RigidBody2D:
		parent.freeze = false

	on_drop.emit(carrier)
	carrier = null


func play_drop_animation(land_pos: Vector2):
	var parent = main_parent as RigidBody2D
	var tween  = create_tween()
	var tweenX = create_tween()
	
	tween.set_parallel().set_ease(Tween.EASE_IN_OUT) 

	tween.tween_property(parent, "global_position:y", parent.global_position.y - 20, 0.2).set_trans(Tween.TRANS_QUAD)
	tweenX.tween_property(parent, "global_position:x", land_pos.x, 0.4)
	tween.tween_property(parent, "scale", Vector2(.5, .5), 0.2).set_trans(Tween.TRANS_QUAD)
	
	tween.chain()
	
	tween.tween_property(parent, "global_position:y", land_pos.y, 0.2).set_trans(Tween.TRANS_SINE)
	tween.tween_property(parent, "scale", Vector2(1,1), 0.2).set_trans(Tween.TRANS_QUAD)
	
	tween.chain()
	
	return tween.finished


func play_pick_up_animation(end_pos: Vector2):
	var parent = main_parent as RigidBody2D
	var tween  = create_tween()
	var tweenX = create_tween()
	
	tween.set_parallel().set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_SINE)

	tween.tween_property(parent, "scale", Vector2(.3, .3), 0.2).set_trans(Tween.TRANS_EXPO)
	tween.tween_property(parent, "position:y", parent.position.y - 20, 0.2)
	tweenX.tween_property(parent, "position:x", end_pos.x, 0.4)
	
	tween.chain()
	
	tween.tween_property(parent, "position:y", end_pos.y, 0.2)
	tween.tween_property(parent, "scale", Vector2(1,1), 0.2).set_trans(Tween.TRANS_EXPO)
	
	return tween.finished


func retire_unc():
	var parent = main_parent as RigidBody2D
	parent.reparent(get_tree().current_scene, true)
	if parent is RigidBody2D:
		parent.freeze = false
		
	is_being_carried = false
	carrier = null


func can_be_carried(_carrier: ICarrier) -> bool:
	return can_be_carried_callable.call(_carrier) and !is_being_carried


func assert_assigned_callables() -> void:
	assert_callable(can_be_carried_callable, "can_be_carried_callable")
