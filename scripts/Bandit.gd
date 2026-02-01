extends CharacterBody2D

@export var speed := 120.0
@export var chase_speed := 180.0
@export var vision_angle := 70.0
@export var vision_range := 220.0
@export var shoot_distance := 200.0
@export var shoot_cooldown := 1.2

@export var min_x := -100.0
@export var max_x := 800.0
@export var min_y := -100.0
@export var max_y := 600.0
@export var roam_wait_time := 1.5

var dir: Vector2
var player
var nav_agent: NavigationAgent2D
var roam_timer := 0.0
var has_target := false
var shoot_timer := 0.0

@onready var sprite: Sprite2D = $Sprite2D
@onready var sight_ray: RayCast2D = $SightRay
@onready var gunshot_sound: AudioStreamPlayer2D = $AudioStreamPlayer2D

func _ready():
	player = get_tree().get_first_node_in_group("player")
	nav_agent = $NavigationAgent2D
	add_to_group("bandits")
	randomize()


func _process(_delta):
	queue_redraw()

func _physics_process(delta):
	shoot_timer -= delta

	if player and can_see_player() and not player.mask_on:
		chase_and_attack()
	else:
		roam(delta)

	if velocity.length() > 5:
		rotation = velocity.angle()
		
	velocity = nav_agent.get_velocity()
	move_and_slide()
	
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
	dir = global_position.direction_to(next_pos)

	var desired_velocity = dir * speed
	velocity = velocity.move_toward(desired_velocity, 800 * delta)

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()


func set_new_roam_target():
	var random_point = Vector2(
		randf_range(min_x, max_x),
		randf_range(min_y, max_y)
	)
	nav_agent.set_target_position(random_point)
	has_target = true

func chase_and_attack():
	nav_agent.target_position = player.global_position

	if nav_agent.is_navigation_finished():
		return

	var next_pos = nav_agent.get_next_path_position()
	dir = global_position.direction_to(next_pos)

	var desired_velocity = dir * chase_speed
	velocity = velocity.move_toward(desired_velocity, 900 * get_physics_process_delta_time())

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()

	try_shoot_player()

func has_line_of_sight_to_player() -> bool:
	var space_state = get_world_2d().direct_space_state
	var from = global_position
	var to = player.global_position

	var query = PhysicsRayQueryParameters2D.create(from, to)
	query.exclude = [self]
	query.collide_with_areas = false
	query.collide_with_bodies = true

	var result = space_state.intersect_ray(query)

	if result and result.collider != player:
		return false  # wall in the way

	return true

func try_shoot_player():
	if shoot_timer > 0:
		return
	
	if not has_line_of_sight_to_player():
		return

	if global_position.distance_to(player.global_position) <= shoot_distance:
		shoot_timer = shoot_cooldown

		if gunshot_sound:
			gunshot_sound.play()

		player.die()

func can_see_player() -> bool:
	if not player:
		return false

	var to_player = player.global_position - global_position
	var dist = to_player.length()

	if dist > vision_range:
		return false

	var forward = Vector2.RIGHT.rotated(rotation)
	var angle_to_player = rad_to_deg(forward.angle_to(to_player.normalized()))
	if abs(angle_to_player) > vision_angle * 0.5:
		return false

	sight_ray.target_position = to_player
	sight_ray.force_raycast_update()

	if sight_ray.is_colliding():
		var hit = sight_ray.get_collider()

		if hit != player:
			return false

	return true

func _draw():
	var cone_color = Color(1, 0, 0, 0.15)
	var half_angle = deg_to_rad(vision_angle * 0.5)

	var points = [Vector2.ZERO]

	var rays := 20
	for i in range(rays + 1):
		var angle = lerp(-half_angle, half_angle, float(i) / rays)
		dir = Vector2.RIGHT.rotated(angle + rotation)
		var end_point = cast_vision_ray(dir)
		points.append(end_point)

	draw_polygon(points, [cone_color])

func cast_vision_ray(direction: Vector2) -> Vector2:
	var space_state = get_world_2d().direct_space_state
	
	var from = global_position
	var to = from + direction * vision_range
	
	var query = PhysicsRayQueryParameters2D.create(from, to)
	query.exclude = [self]
	query.collide_with_areas = false
	query.collide_with_bodies = true
	
	var result = space_state.intersect_ray(query)

	if result:
		return to_local(result.position)
	else:
		return to_local(to)
