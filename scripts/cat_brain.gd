extends CharacterBody2D

@export var speed := 120.0
@export var vision_angle := 70.0
@export var vision_range := 220.0
@export var roam_wait_time := 1.5

@export var min_x := -100.0
@export var max_x := 800.0
@export var min_y := -100.0
@export var max_y := 600.0

var nav_agent: NavigationAgent2D
var roam_timer := 0.0
var has_target := false
var last_facing := "down"
var look_direction := Vector2.RIGHT
var rng = RandomNumberGenerator.new()

@onready var anim: AnimatedSprite2D = %AnimatedSprite2D
@onready var meow_sound: AudioStreamPlayer2D = %AudioStreamPlayer2D
@onready var meow_timer : Timer = %MeowTimer
func _ready():
	nav_agent = $NavigationAgent2D
	randomize()

func meow():
	var meow_time_seconds_between = rng.randf_range(-10.0, 10.0)
	meow_timer.start(meow_time_seconds_between)
	

func set_new_roam_target():
	var random_point = Vector2(
		randf_range(min_x, max_x),
		randf_range(min_y, max_y)
	)
	nav_agent.target_position = random_point
	has_target = true

func _physics_process(delta: float) -> void:
	velocity = nav_agent.get_velocity()
	roam(delta)
	
	var moving = velocity.length() > 10

	if abs(velocity.y) > abs(velocity.x):
		last_facing = "up" if velocity.y < 0 else "down"
	
	if not anim.is_playing():
		anim.play(last_facing + "_idle")
	
	if moving:
		anim.play("walk_" + last_facing)
	else:
		anim.play("idle_" + last_facing)

	anim.flip_h = velocity.x < 0

func roam(delta):
	if not has_target:
		roam_timer -= delta
		if roam_timer <= 0:
			set_new_roam_target()
			roam_timer = roam_wait_time
		velocity = Vector2.ZERO
		return

	if nav_agent.is_navigation_finished():
		has_target = false
		velocity = Vector2.ZERO
		return

	var next_pos = nav_agent.get_next_path_position()
	var dir = global_position.direction_to(next_pos)
	var desired_velocity = dir * speed
	velocity = velocity.move_toward(desired_velocity, 800 * delta)

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()


func _on_meow_timer_timeout() -> void:
	meow_sound.play()
