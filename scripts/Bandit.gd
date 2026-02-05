extends CharacterBody2D

@export var speed := 120.0
@export var chase_speed := 180.0
@export var vision_angle := 70.0
@export var vision_range := 220.0
@export var shoot_distance := 200.0
@export var shoot_cooldown := 1.2
@export var roam_wait_time := 1.5
@export var hearing_radius := 160.0
@export var investigate_time := 2.0

@export var min_x := -100.0
@export var max_x := 800.0
@export var min_y := -100.0
@export var max_y := 600.0

var player
var nav_agent: NavigationAgent2D
var roam_timer := 0.0
var has_target := false
var shoot_timer := 0.0
var investigate_timer := 0.0
var investigate_target := Vector2.ZERO
var is_investigating := false
var last_facing := "down"
var is_shooting := false
var player_eliminated := false
var look_direction := Vector2.RIGHT
var is_global_alert := false

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var sight_ray: RayCast2D = $SightRay
@onready var gunshot_sound: AudioStreamPlayer2D = $AudioStreamPlayer2D

func _ready():
	GameManager.on_max_suspicion.connect(_on_global_alert)
	GameManager.on_max_items_stolen.connect(_on_global_alert)
	player = get_tree().get_first_node_in_group("player")
	nav_agent = $NavigationAgent2D
	add_to_group("bandits")
	randomize()
	update_animation()

func _process(_delta):
	queue_redraw()

func _physics_process(delta):
	shoot_timer -= delta

	if player_eliminated:
		velocity = Vector2.ZERO
		move_and_slide()
		update_animation()
		return

	if is_global_alert and player and not player.is_dead:
		chase_player_globally()
	elif player and can_see_player() and not player.mask_on:
		is_investigating = false
		chase_and_attack()
	elif player and can_hear_player() and not player.mask_on:
		start_investigating(player.global_position)
	elif is_investigating:
		investigate(delta)
	else:
		roam(delta)



	# Update look direction for vision
	if velocity.length() > 5:
		look_direction = velocity.normalized()

	velocity = nav_agent.get_velocity()
	move_and_slide()
	update_animation()

func _on_global_alert():
	is_global_alert = true
	is_investigating = false
	has_target = false
	print("BANDIT IS NOW ASS MAD")

func chase_player_globally():
	nav_agent.target_position = player.global_position

	if nav_agent.is_navigation_finished():
		return

	var next_pos = nav_agent.get_next_path_position()
	var dir = global_position.direction_to(next_pos)

	var desired_velocity = dir * chase_speed * 1.3  # faster when mad
	velocity = velocity.move_toward(desired_velocity, 1100 * get_physics_process_delta_time())

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()

	try_shoot_player()

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

func start_investigating(pos: Vector2):
	investigate_target = pos
	nav_agent.target_position = pos
	is_investigating = true
	investigate_timer = investigate_time

func investigate(delta):
	var distance_to_target = global_position.distance_to(investigate_target)

	if distance_to_target < 14:
		velocity = Vector2.ZERO
		nav_agent.set_velocity(Vector2.ZERO)
		nav_agent.target_position = global_position
		investigate_timer -= delta
		if investigate_timer <= 0:
			is_investigating = false
		return

	var next_pos = nav_agent.get_next_path_position()
	var dir = global_position.direction_to(next_pos)
	var desired_velocity = dir * speed * 0.7
	velocity = velocity.move_toward(desired_velocity, 600 * delta)

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()

func can_hear_player() -> bool:
	if not player:
		return false
	if player.velocity.length() < 160:
		return false
	return global_position.distance_to(player.global_position) <= hearing_radius

func set_new_roam_target():
	var random_point = Vector2(
		randf_range(min_x, max_x),
		randf_range(min_y, max_y)
	)
	nav_agent.target_position = random_point
	has_target = true

func chase_and_attack():
	nav_agent.target_position = player.global_position

	if nav_agent.is_navigation_finished():
		return

	var next_pos = nav_agent.get_next_path_position()
	var dir = global_position.direction_to(next_pos)
	var desired_velocity = dir * chase_speed
	velocity = velocity.move_toward(desired_velocity, 900 * get_physics_process_delta_time())

	nav_agent.set_velocity(velocity)
	velocity = nav_agent.get_velocity()

	try_shoot_player()

func has_line_of_sight_to_player() -> bool:
	var space_state = get_world_2d().direct_space_state
	var query = PhysicsRayQueryParameters2D.create(global_position, player.global_position)
	query.exclude = [self]
	query.collide_with_areas = false
	query.collide_with_bodies = true
	var result = space_state.intersect_ray(query)
	return not (result and result.collider != player)

func try_shoot_player():
	if shoot_timer > 0:
		return
	if not has_line_of_sight_to_player():
		return
	if global_position.distance_to(player.global_position) <= shoot_distance:
		shoot_timer = shoot_cooldown
		player_eliminated = true
		is_shooting = true
		anim.play("shoot_anim")
		if gunshot_sound:
			gunshot_sound.play()
		player.die()

func can_see_player() -> bool:
	if not player:
		return false

	var to_player = player.global_position - global_position
	if to_player.length() > vision_range:
		return false

	var angle_to_player = rad_to_deg(look_direction.angle_to(to_player.normalized()))
	if abs(angle_to_player) > vision_angle * 0.5:
		return false

	sight_ray.target_position = to_player
	sight_ray.force_raycast_update()

	if sight_ray.is_colliding() and sight_ray.get_collider() != player:
		return false

	return true

func _draw():
	var cone_color = Color(1, 0, 0, 0.15)
	var half_angle = deg_to_rad(vision_angle * 0.5)
	var points = [Vector2.ZERO]
	var rays := 20

	for i in range(rays + 1):
		var angle = lerp(-half_angle, half_angle, float(i) / rays)
		var dir = look_direction.rotated(angle)
		points.append(cast_vision_ray(dir))

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
	return to_local(result.position if result else to)

func update_animation():
	if is_shooting:
		if not anim.is_playing():
			is_shooting = false
			anim.play("idle_" + last_facing)
		else:
			return

	var moving = velocity.length() > 10

	if moving:
		if velocity.y < 0:
			last_facing = "up"
		elif velocity.y > 0:
			last_facing = "down"
		else:
			last_facing = "side"

	if moving:
		anim.play("walk_" + last_facing)
	else:
		anim.play("idle_" + last_facing)

	anim.flip_h = velocity.x < 0
