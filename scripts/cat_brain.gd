extends CharacterBody2D

@export var speed := 80.0
@export var roam_wait_time := 1.0

@export var min_x := -100.0
@export var max_x := 800.0
@export var min_y := -100.0
@export var max_y := 600.0

@export var doCatTimes := 5

var nav_agent: NavigationAgent2D
var roam_timer := 0.0
var has_target := false
var last_facing := "front"
var look_direction := Vector2.RIGHT
var rng = RandomNumberGenerator.new()

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var meow_sound: AudioStreamPlayer2D = $MeowSound
@onready var meow_timer : Timer = $MeowTimer
@onready var interactable: IInteractible = $IInteractable

func _ready():
	nav_agent = $NavigationAgent2D
	randomize()
	start_meow_timer()

func start_meow_timer():
	meow_timer.start(rng.randf_range(5.0, 15.0))
	
func set_new_roam_target():
	var random_point = Vector2(
		randf_range(min_x, max_x),
		randf_range(min_y, max_y)
	)
	nav_agent.target_position = random_point
	has_target = true

func _physics_process(delta: float) -> void:
	roam(delta)
	velocity = nav_agent.get_velocity()
	
	move_and_slide()
	
	var moving = velocity.length() > 10
	
	
	if moving:
		if velocity.y < 0:
			last_facing = "back"
		elif velocity.y > 0:
			last_facing = "front"
		else:
			last_facing = "side"
	
	if not anim.is_playing():
		anim.play(last_facing + "_idle")
	
	if moving:
		anim.play(last_facing + "_run")
	else:
		anim.play(last_facing + "_idle")
	
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
	start_meow_timer()

func _on_interactable_object_on_interact_finish():
	GameManager.complete_objective(ObjectiveModel.Objective.FeedKitty)


func _on_i_interactable_on_interaction(iInteractor):
	doCatTimes -= 1
	
	if doCatTimes <= 0:
		GameManager.complete_objective(ObjectiveModel.Objective.FeedKitty)
		interactable.update_can_interact_status()

func can_interact(_interactor: IInteractor):
	if GameManager.current_objective == ObjectiveModel.Objective.FeedKitty:
		return doCatTimes > 0
