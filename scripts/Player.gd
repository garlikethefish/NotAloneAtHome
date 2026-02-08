extends CharacterBody2D
class_name PlayerCharacter

@export var speed := 100.0
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
@onready var carrier: ICarrier = $ICarrier
@onready var interactor: IInteractor = $IInteractor
@onready var overlay_rect: ColorRect = $MaskOverlay/ColorRect
@onready var footstep_sound : AudioStreamPlayer2D = $FootstepSound
@onready var mask_sound : AudioStreamPlayer2D = $Breathe
@onready var breathing_particles : GPUParticles2D = $BreathingParticles

var last_facing := "down"
var wait := false
var sprinting := false
var wait_particles := false

func _ready():
	GameManager.player = self
	can_move = GameManager.player_can_move
	add_to_group("player")
	
	if overlay_rect:
		overlay_rect.visible = true
		if overlay_rect.material:
			overlay_rect.material.set_shader_parameter("center", Vector2(0.5, 0.5))
			overlay_rect.material.set_shader_parameter("radius", atmosphere_radius)

func _process(delta):
	can_move = GameManager.player_can_move
	
	if direction != Vector2.ZERO:
		carrier.facingDirection = velocity.normalized()
		if footstep_sound.playing == false and wait == false:
			play_footstep_sound()
	
	if Input.is_action_just_pressed("interact"):
		var interactable: IInteractible = interactor.iInteractable
		var carriable: ICariable = null
		interactor.interact()
		
		if interactable:
			var parent := interactor.iInteractable.get_parent()
			carriable = Utils.try_get_child_of_type(parent, ICariable)
			
		carrier.try_to_carry(carriable)
	
	if overlay_rect and overlay_rect.material:
		var mat = overlay_rect.material
		mat.set_shader_parameter("center", Vector2(0.5, 0.5))

		if mask_on:
			current_radius = max(min_vision_radius, current_radius - vision_shrink_speed * delta)
			if breathing_particles.emitting == false:
				play_breathing_particles()
			if mask_sound.playing == false:
				mask_sound.play()
		else:
			mask_sound.stop()
			current_radius = min(atmosphere_radius, current_radius + vision_expand_speed * delta)

		mat.set_shader_parameter("radius", current_radius)

func _physics_process(_delta):
	direction = Vector2.ZERO
	direction.x = Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
	direction.y = Input.get_action_strength("move_down") - Input.get_action_strength("move_up")
	direction = direction.normalized()

	var current_speed = speed

	if Input.is_action_pressed("sprint"):
		sprinting = true
		current_speed *= sprint_multiplier
	else:
		sprinting = false
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

func play_footstep_sound():
	footstep_sound.play()
	wait = true
	if sprinting:
		await get_tree().create_timer(0.4).timeout
	else:
		await get_tree().create_timer(0.5).timeout
	wait = false
	
func play_breathing_particles():
	breathing_particles.emitting = true
	wait_particles = true
	if sprinting:
		await get_tree().create_timer(0.8).timeout
	else:
		await get_tree().create_timer(1.0).timeout
	wait_particles = false
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
		carrier.carry_stop()
		current_radius = max_vision_radius
	
	interactor.on_interactor_status_update.emit()


func _on_i_interactor_on_interactable_change(_iInteractable: IInteractible):
	if !_iInteractable: return
	
func can_carry(_cariable: ICariable) -> bool:
	return !mask_on and !carrier.isCarrying

func can_interact(_interactable: IInteractible):
	var carriable: ICariable = Utils.try_get_child_of_type(_interactable.get_parent(), ICariable)
	
	if Utils.try_get_parent_of_type(_interactable, DeadThiefCloset):
		return !mask_on 
		
	return can_carry(carriable)
