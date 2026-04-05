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
var can_move: bool: 
	get: return (
		!thrower.is_charging 
		and GameManager.player_can_move 
		and !carrier.is_animating
	)
var bandit_near := false
var bandit_body
var distance

var direction: Vector2 = Vector2.ZERO 


@onready var helper_holder: HelperHolder = $HelperHolder
@onready var thrower: IThrower       = helper_holder.get_helper(IThrower)
@onready var interactor: IInteractor = helper_holder.get_helper(IInteractor)
@onready var carrier: ICarrier       = helper_holder.get_helper(ICarrier)
@onready var detector: IProximityAreaDetector = helper_holder.get_helper(IProximityAreaDetector)


@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var overlay_rect: ColorRect = $MaskOverlay/ColorRect
@onready var footstep_sound : AudioStreamPlayer2D = $FootstepSound
@onready var mask_sound : AudioStreamPlayer2D = $Breathe
@onready var breathing_particles : GPUParticles2D = $BreathingParticles
@onready var nearness_detector : Area2D = $BanditNearnessDetector

var last_facing := "down"
var wait := false
var sprinting := false
var wait_particles := false
var shaking_camera := false

signal shake_camera
signal stop_camera_shake

func _ready():
	interactor.can_interact_callable = can_interact
	carrier.can_carry_callable = can_carry
	
	GameManager.player = self
	add_to_group("player")
	
	if overlay_rect:
		overlay_rect.visible = true
		if overlay_rect.material:
			overlay_rect.material.set_shader_parameter("center", Vector2(0.5, 0.5))
			overlay_rect.material.set_shader_parameter("radius", atmosphere_radius)

func _process(delta):
	thrower.set_target_direction(get_global_mouse_position() - self.global_position)
	
	if Input.is_action_pressed("throw"):
		# start charge
		if carrier.carriable:
			var throwable: IThrowable = carrier.carriable.get_fellow_helper(IThrowable)
			if !thrower.try_start_charge(throwable): pass
		
	if Input.is_action_just_released("throw"):
		# stop charge & throw
		if !thrower.try_throw(): push_warning("Failed to throw!")
		
	if Input.is_action_just_pressed("drop"):
		carrier.try_to_drop()
		
	if Input.is_action_just_pressed("interact"):
		var prioriy = detector.closest_detectable
		
		var carriable: ICarriable = null
		if prioriy:
			carriable = prioriy.get_fellow_helper(ICarriable)
		
		carrier.try_to_carry(carriable)
		
		if prioriy:  
			var interactable: IInteractible = prioriy.get_fellow_helper(IInteractible)
			if interactable:
				interactor.interact(interactable)
				
	if can_move:
		if direction != Vector2.ZERO:
			carrier.facingDirection = velocity.normalized()
			if footstep_sound.playing == false and wait == false:
				play_footstep_sound()
	
		if shaking_camera:
				emit_signal("stop_camera_shake")
		
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
	else: # is programming
		if nearness_detector.get_overlapping_areas() != null:
			for body in nearness_detector.get_overlapping_areas():
				if body.get_parent().name == "Bandit":
					bandit_body = body.global_position
					bandit_near = true
					distance = bandit_body.distance_to(self.position)
		if bandit_near and distance <= 300:
			if !shaking_camera:
				emit_signal("shake_camera")
			shaking_camera = true
			# start screenshake
		else:
			if shaking_camera:
				emit_signal("stop_camera_shake")
			shaking_camera = false
		immobile_animation()

func _physics_process(_delta):
	if can_move:
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
		
func immobile_animation():
	last_facing = "up"
	var mask_suffix = "_mask" if mask_on else ""
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
	emit_signal("stop_camera_shake")
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
	if not has_mask or (carrier.is_carrying and !carrier.try_to_drop()):
		return

	mask_on = !mask_on

	if mask_on:
		current_radius = max_vision_radius


func can_carry(_cariable: ICarriable) -> bool:
	return !mask_on and !carrier.is_carrying

func can_interact(_interactable: IInteractible) -> bool:
	# exclusion for dead thief and closet
	if (
		_interactable != null
		and _interactable.main_parent is DeadThiefCloset 
		and carrier.carriable
		and carrier.carriable.main_parent is DeadThief
	):
		return !mask_on
	
	return !mask_on and !carrier.is_carrying


func _on_i_thrower_on_throw(_thrower: IThrower) -> void:
	carrier.retire_unc()


func _on_i_carrier_on_carry_stop(_carriable: ICarriable) -> void:
	thrower.remove_throwable()
