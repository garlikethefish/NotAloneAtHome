extends Helper

class_name ICarrier

signal on_carry_start(carriable: ICarriable)
signal on_carry_stop(carriable: ICarriable)

@export var excluded_colliders: Array[CollisionObject2D] = []
@export var reach: float = 40.0      # How far we check
@export var step_size: float = 5.0   # Density of "fat" checks
@export var start_offset: float = 12.0 # Jump past the player's own collision
@export var ray_count = 16
@export var show_debug := true
@export var collision_masks: Array[int] = []

@onready var carry_point: Node2D = $CarryPoint

var debug_points = [] # Stores {pos: Vector2, is_valid: bool}
var debug_rays = []   # Stores {from: Vector2, to: Vector2}

var is_picking_up := false
var is_dropping   := false
var is_animating: bool:
	get(): return is_picking_up or is_dropping

var is_carrying: bool:
	get: return carriable != null
	
var can_carry_callable: Callable
var carriable:          ICarriable
var facingDirection               := Vector2.INF
var valid_carriable_drop_position := Vector2.INF


func _draw():
	# 1. Draw the scout rays
	for ray in debug_rays:
		draw_line(to_local(ray.from), to_local(ray.to), Color(0, 1, 1, 0.3), 1.0)
	
	# 2. Draw the "Spam" dots
	for p in debug_points:
		var color = Color.GREEN if p.is_valid else Color.RED
		draw_circle(to_local(p.pos), 3.0, color)
	

	if valid_carriable_drop_position != Vector2.INF:
		var local_pos = to_local(valid_carriable_drop_position)

		draw_circle(local_pos, 6.0, Color.YELLOW)
		draw_arc(local_pos, 10, 0, TAU, 32, Color.YELLOW, 2.0)


func find_placement_hybrid():
	debug_points.clear()
	debug_rays.clear()
	
	var current_best_location   := Vector2.INF
	var space_state             = get_world_2d().direct_space_state
	var directions              = []
	var excluded_collider_rids  = excluded_colliders.map(func (item: CollisionObject2D): return item.get_rid())
	var combined_mask           = Utils.get_combined_mask(collision_masks)
	var shape_to_check: Shape2D = carriable.cs.shape  # my_object_shape

	# Set ray directions around a point
	for i in range(ray_count):
		# TAU is a built-in Godot constant equal to 2 * PI (a full circle)
		var angle = i * (TAU / ray_count) 
		directions.append(Vector2.RIGHT.rotated(angle))

	for dir in directions:
		var ray_start = global_position + (dir * start_offset)
		var ray_end   = global_position + (dir * reach)
		
		var ray_query = PhysicsRayQueryParameters2D.create(ray_start, ray_end)
		ray_query.exclude = excluded_collider_rids
		ray_query.collision_mask = combined_mask
		ray_query.hit_from_inside = true
		
		var ray_result = space_state.intersect_ray(ray_query)
		var ray_traveled_distance = reach
		if ray_result:
			ray_traveled_distance = global_position.distance_to(ray_result.position)
		
		# Save ray for debug drawing
		debug_rays.append({"from": ray_start, "to": global_position + dir * ray_traveled_distance})

		# checking if object fits int current step
		# ray ----x---x----x----x  x - step (distance) traveled from start 2 -> 4 -> 6
		for step_distance_in_ray in range(start_offset, ray_traveled_distance, step_size):
			var test_pos = global_position + dir * step_distance_in_ray
			
			var shape_query = PhysicsShapeQueryParameters2D.new()
			shape_query.shape = shape_to_check
			shape_query.transform = Transform2D(0, test_pos)
			shape_query.collision_mask = combined_mask
			shape_query.exclude = excluded_collider_rids
			
			var overlaps = space_state.intersect_shape(shape_query)
			var is_free = overlaps.is_empty()
			
			debug_points.append({"pos": test_pos, "is_valid": is_free})
			
			if is_free:
				if current_best_location.distance_to(global_position) > test_pos.distance_to(global_position):
					current_best_location = test_pos
			
	valid_carriable_drop_position = current_best_location
	
	
func _process(_delta):
	if show_debug: queue_redraw()

	if carriable:
		find_placement_hybrid()


func toggle_carry(_cariable: ICarriable):
	if !is_carrying:
		try_carry_start(_cariable)
	else:
		carry_stop()

func carry_stop(show_animation: bool = true):
	if (
		!carriable 
		or valid_carriable_drop_position == Vector2.INF
		or is_animating
	): return
	
	is_dropping = true
	carriable.on_drop.connect(func (_carrier):
		is_dropping = false, 
		CONNECT_ONE_SHOT
	)
	carriable.drop(valid_carriable_drop_position)
	on_carry_stop.emit(carriable)
	carriable = null
	
func try_carry_start(_cariable: ICarriable) -> bool:
	if (
		_cariable == null 
		or is_carrying
		or !_cariable.can_be_carried(self) 
		or !can_carry(_cariable)
		or is_animating
	): return false
	
	is_picking_up = true
	_cariable.on_pick_up.connect(func (_carrier):
		is_picking_up = false
		print("Picked up"), 
		CONNECT_ONE_SHOT
	)
	
	# cancels active pos tween
	_cariable.get_position_tween().kill()
	carriable = _cariable
	carriable.pick_up(self)
	on_carry_start.emit(_cariable)
	return true
	
func retire_unc():
	if carriable:
		carriable.retire_unc()
		carriable = null
	
func can_carry(_carriable: ICarriable) -> bool:
	return can_carry_callable.call(_carriable)
	
func assert_assigned_callables() -> void:
	assert_callable(can_carry_callable, "can_carry_callable")
