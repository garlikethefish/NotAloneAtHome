extends CharacterBody2D

class_name PlayerCharacter

@export var speed := 200.0
@export var sprint_multiplier := 2

var has_mask := false
var mask_on := false

@onready var mask_overlay = $MaskOverlay/ColorRect

func _ready():
	if mask_overlay:
		mask_overlay.modulate.a = 0.0

func _physics_process(_delta):
	var direction = Vector2.ZERO

	direction.x = Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	direction.y = Input.get_action_strength("move_down") - Input.get_action_strength("move_up")
	direction = direction.normalized()

	var current_speed = speed

	if Input.is_action_pressed("sprint"):
		current_speed *= sprint_multiplier

	velocity = direction * current_speed
	move_and_slide()

	if Input.is_action_just_pressed("toggle_mask"):
		toggle_mask()

func add_mask():
	has_mask = true
	print("You picked up a mask!")

func toggle_mask():
	if not has_mask:
		print("You don't have a mask")
		return

	mask_on = !mask_on
	update_mask_visual()

	if mask_on:
		print("Mask ON")
	else:
		print("Mask OFF")

func update_mask_visual():
	if not mask_overlay:
		return

	if mask_on:
		$Sprite2D.modulate = Color(0.7, 0.7, 0.7)
		mask_overlay.modulate.a = 0.4
	else:
		$Sprite2D.modulate = Color(1, 1, 1)
		mask_overlay.modulate.a = 0.0
