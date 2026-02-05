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
var is_dead := false
var has_mask := false
var mask_on := false
var isCarringObject := false
var carriableObject: InteractableObject
var can_move := true

var direction: Vector2 = Vector2.ZERO 

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var overlay_rect: ColorRect = $MaskOverlay/ColorRect

var last_facing := "down"

func _ready():
	can_move = GameManager.player_can_move
	add_to_group("player")
	
	if overlay_rect:
		overlay_rect.visible = true
		if overlay_rect.material:
			overlay_rect.material.set_shader_parameter("center", Vector2(0.5, 0.5))
			overlay_rect.material.set_shader_parameter("radius", atmosphere_radius)

func _process(delta):
	can_move = GameManager.player_can_move
	if overlay_rect and overlay_rect.material:
		var mat = overlay_rect.material
		mat.set_shader_parameter("center", Vector2(0.5, 0.5))

		if mask_on:
			current_radius = max(min_vision_radius, current_radius - vision_shrink_speed * delta)
		else:
			current_radius = min(atmosphere_radius, current_radius + vision_expand_speed * delta)

		mat.set_shader_parameter("radius", current_radius)

func _physics_process(_delta):
	direction = Vector2.ZERO
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

	if is_dead:
		return

func update_animation(dir: Vector2):
	var moving = dir.length() > 0

	if dir.x > 0:
		anim.flip_h = false
	elif dir.x < 0:
		anim.flip_h = true
		
	if moving:
		if dir.y < 0:
			last_facing = "up"
		elif dir.y > 0:
			last_facing = "down"
		else:
			last_facing = "side"

	var mask_suffix = "_mask" if mask_on else ""

	if moving:
		anim.play("walk_" + last_facing + mask_suffix)
	else:
		anim.play("idle_" + last_facing + mask_suffix)

func add_mask():
	has_mask = true

func show_defeat_screen(reason: String):
	var defeat_scene = preload("res://scenes/DefeatScreen.tscn").instantiate()
	get_tree().current_scene.add_child(defeat_scene)
	defeat_scene.set_defeat_reason(reason)

func die():
	if is_dead:
		return

	is_dead = true
	print("You were shot!")

	velocity = Vector2.ZERO
	set_physics_process(false)
	set_process(false)

	Engine.time_scale = 0.4
	await get_tree().create_timer(0.6).timeout
	Engine.time_scale = 1.0

	show_defeat_screen("YOU WERE SHOT")

func toggle_mask():
	if not has_mask:
		return

	mask_on = !mask_on

	if mask_on:
		current_radius = max_vision_radius
