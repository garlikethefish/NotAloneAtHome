extends CharacterBody2D
class_name PlayerCharacter

@export var speed := 200.0
@export var sprint_multiplier := 2.0
@export var mask_speed_multiplier := 0.6

@export var max_vision_radius := 0.35
@export var min_vision_radius := 0.08
@export var vision_shrink_speed := 0.05
@export var atmosphere_radius := 0.35
@export var vision_expand_speed := 0.25

var current_radius := 0.35
var has_mask := false
var mask_on := false
var isCarringObject := false

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var overlay_rect: ColorRect = $MaskOverlay/ColorRect

var last_facing := "down"

func _ready():
	if overlay_rect:
		overlay_rect.visible = true
		if overlay_rect.material:
			overlay_rect.material.set_shader_parameter("center", Vector2(0.5, 0.5))
			overlay_rect.material.set_shader_parameter("radius", atmosphere_radius)

func _process(delta):
	if overlay_rect and overlay_rect.material:
		var mat = overlay_rect.material
		mat.set_shader_parameter("center", Vector2(0.5, 0.5))

		if mask_on:
			current_radius = max(min_vision_radius, current_radius - vision_shrink_speed * delta)
		else:
			current_radius = min(atmosphere_radius, current_radius + vision_expand_speed * delta)

		mat.set_shader_parameter("radius", current_radius)




func _physics_process(_delta):
	var direction = Vector2.ZERO
	direction.x = Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	direction.y = Input.get_action_strength("move_down") - Input.get_action_strength("move_up")
	direction = direction.normalized()

	var current_speed = speed

	if Input.is_action_pressed("sprint"):
		current_speed *= sprint_multiplier

	if mask_on:
		current_speed *= mask_speed_multiplier

	velocity = direction * current_speed
	move_and_slide()

	update_animation(direction)

	if Input.is_action_just_pressed("toggle_mask"):
		toggle_mask()


func update_animation(direction: Vector2):
	var moving = direction.length() > 0

	if direction.x > 0:
		anim.flip_h = false
	elif direction.x < 0:
		anim.flip_h = true

	if moving:
		if abs(direction.y) > abs(direction.x):
			last_facing = "up" if direction.y < 0 else "down"

	var mask_suffix = "_mask" if mask_on else ""

	if moving:
		anim.play("walk_" + last_facing + mask_suffix)
	else:
		anim.play("idle_" + last_facing + mask_suffix)

func add_mask():
	has_mask = true

func toggle_mask():
	if not has_mask:
		return

	mask_on = !mask_on

	if mask_on:
		current_radius = max_vision_radius

func update_mask_visual():
	if overlay_rect:
		overlay_rect.visible = mask_on
