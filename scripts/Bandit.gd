extends CharacterBody2D

enum State { PATROL, FOLLOW, SUSPICIOUS, CHASE, FRIENDLY, ALERT, INVESTIGATE }

@export var speed := 120.0
@export var chase_speed := 180.0
@export var suspicion_distance := 120.0
@export var alert_distance := 70.0
@export var investigate_speed := 140.0
@export var patrol_points: Array[Vector2]
@export var follow_distance := 180.0
@export var follow_buffer := 20.0
@export var suspicion_buffer := 15.0
@export var vision_angle := 70.0
@export var vision_range := 220.0
@export var shoot_distance := 200.0
@export var shoot_cooldown := 1.2


var state = State.PATROL
var player
var investigate_target := Vector2.ZERO
var current_patrol_index := 0
var suspicion_timer := 0.0
var shoot_timer := 0.0
var is_dead := false


@onready var anim: Sprite2D = $Sprite2D
@onready var vision_area: Area2D = $VisionArea
@onready var gunshot_sound: AudioStreamPlayer2D = $AudioStreamPlayer2D


func _ready():
	player = get_tree().get_first_node_in_group("player")
	vision_area.body_entered.connect(_on_body_entered)
	vision_area.body_exited.connect(_on_body_exited)
	add_to_group("bandits")

	anim.rotation_degrees = 90

func _process(_delta):
	queue_redraw()

func _physics_process(delta):
	if player and state != State.INVESTIGATE:
		var dist = global_position.distance_to(player.global_position)
		var sees_player = can_see_player()

		if not sees_player:
			state = State.PATROL
		else:
			if player.mask_on:
				state = State.PATROL
			else:
				if dist <= shoot_distance:
					state = State.CHASE

	match state:
		State.PATROL:
			patrol()
		State.FOLLOW:
			follow_player()
		State.SUSPICIOUS:
			suspicious_behavior(delta)
		State.CHASE:
			chase_player()
		State.FRIENDLY:
			idle()
		State.ALERT:
			alert_behavior()
		State.INVESTIGATE:
			investigate_behavior()

	if velocity.length() > 5:
		rotation = velocity.angle()

	move_and_slide()

func can_see_player() -> bool:
	if not player:
		return false

	var to_player = player.global_position - global_position
	var dist = to_player.length()

	if dist > vision_range:
		return false

	var forward = transform.x.normalized()
	var angle_to_player = rad_to_deg(forward.angle_to(to_player.normalized()))

	return abs(angle_to_player) <= vision_angle * 0.5

func patrol():
	if patrol_points.is_empty():
		velocity = Vector2.ZERO
		return

	var target = patrol_points[current_patrol_index]
	var dir = (target - global_position).normalized()
	velocity = dir * speed

	if global_position.distance_to(target) < 10:
		current_patrol_index = (current_patrol_index + 1) % patrol_points.size()

func follow_player():
	if not player:
		return

	var dir = (player.global_position - global_position).normalized()
	velocity = dir * (speed * 0.6)

func suspicious_behavior(delta):
	if not player:
		return

	var dir = (player.global_position - global_position).normalized()
	velocity = dir * (speed * 0.4)

	suspicion_timer += delta

	if suspicion_timer > 2.5:
		suspicion_timer = 0.0

	if global_position.distance_to(player.global_position) < alert_distance:
		state = State.ALERT

func alert_behavior():
	if not player:
		return

	var dir = (player.global_position - global_position).normalized()
	velocity = dir * chase_speed

func chase_player():
	if not player:
		return

	var dir = (player.global_position - global_position).normalized()
	velocity = dir * chase_speed

	try_shoot_player()

func try_shoot_player():
	if shoot_timer > 0:
		return

	var dist = global_position.distance_to(player.global_position)

	if dist <= shoot_distance:
		shoot_timer = shoot_cooldown
		
		if gunshot_sound:
			gunshot_sound.play()
		
		player.die()

func idle():
	velocity = Vector2.ZERO

func investigate_behavior():
	var dir = (investigate_target - global_position).normalized()
	velocity = dir * investigate_speed

	if global_position.distance_to(investigate_target) < 10:
		state = State.PATROL

func _on_body_entered(body):
	if not body.is_in_group("player"):
		return

	var dist = global_position.distance_to(body.global_position)

	if body.mask_on:
		if dist < suspicion_distance:
			state = State.SUSPICIOUS
		else:
			state = State.FRIENDLY
	else:
		state = State.CHASE

func _on_body_exited(body):
	if body.is_in_group("player"):
		state = State.PATROL

func _draw():
	var cone_color = Color(1, 0, 0, 0.15)

	var half_angle = deg_to_rad(vision_angle * 0.5)

	var left_dir = Vector2.RIGHT.rotated(-half_angle) * vision_range
	var right_dir = Vector2.RIGHT.rotated(half_angle) * vision_range

	draw_polygon([Vector2.ZERO, left_dir, right_dir], [cone_color])
