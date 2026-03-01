extends Helper

class_name IThrowable

signal on_throw(throwable: IThrowable)
signal on_land(throwable: IThrowable)

@export var sprite: Sprite2D
var land_position := Vector2.ZERO
var throw_start_position := Vector2.ZERO
var fly_duration := .6
var is_flying := false

func land():
	print("LANDED!!!")
	is_flying = false
	reset_throwable()
	on_land.emit(self)
	
func throw(to_position: Vector2):
	land_position = to_position
	throw_start_position = helper_holder.main_parent.global_position 
	is_flying = true
	start_fly_animation()
	on_throw.emit(self)
	
func start_fly_animation():
	if land_position == Vector2.ZERO: return
	
	var parent = helper_holder.main_parent
	
	get_position_tween().tween_property(parent, "global_position", land_position, fly_duration).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	if sprite:
		var original_y = sprite.position.y
		var original_scale = sprite.scale
		var sprite_tween = create_tween()
		var rotation_tween = create_tween()
		#get_sprite_tween()
		sprite_tween.set_parallel().set_ease(Tween.EASE_IN_OUT) 

		rotation_tween.tween_property(sprite, "rotation", deg_to_rad(360), fly_duration)
		sprite_tween.tween_property(sprite, "position:y", sprite.position.y - 25, fly_duration / 2).as_relative()
		sprite_tween.tween_property(sprite, "scale", Vector2(0.7, 0.7), fly_duration / 2)

		sprite_tween.chain()

		sprite_tween.tween_property(sprite, "position:y", sprite.position.y + 25, fly_duration / 2).as_relative()
		sprite_tween.tween_property(sprite, "scale", original_scale, fly_duration / 2)

		rotation_tween.tween_callback(func(): 
			sprite.rotation = 0
			land()
		)

func reset_throwable():
	land_position = Vector2.ZERO
	throw_start_position = Vector2.ZERO
