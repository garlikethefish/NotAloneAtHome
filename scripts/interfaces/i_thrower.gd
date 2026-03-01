extends Helper

class_name IThrower

signal on_throw(thrower: IThrower)
signal on_throw_charge_start(thrower: IThrower)
signal on_throw_charge_cancel(thrower: IThrower)

var throwable: IThrowable
var normalized_desired_throw_direction := Vector2.ZERO # direction where ur aiming
var current_throw_position := Vector2.ZERO # actual position in that direction
var max_throw_range := 100.0
var is_charging := false
var current_charge := 0.0
var max_charge_seconds := 1.0
var charge_multiplier := 1.0

@export var show_debug := false

@onready var target_sprite_node := $TargetNode
@onready var carrier: ICarrier = helper_holder.get_helper(ICarrier)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	target_sprite_node.visible = false 
	carrier.on_carry_stop.connect(func (carriable: ICarriable):
		var _trowable: IThrowable = carriable.get_helper_from_holder(IThrowable)
		
		if _trowable == throwable:
			remove_throwable()
	)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if show_debug: queue_redraw()
	update_ui()
	
	if !is_charging : return #or !throwable
	
	current_charge = clamp(current_charge + delta * charge_multiplier, 0, max_charge_seconds) 
	
	var calculated_throw_vector = normalized_desired_throw_direction * max_throw_range * get_normalized_charge()
	current_throw_position = global_position + calculated_throw_vector
	
	current_throw_position = get_circle_shot_center(current_throw_position, 12)
	
func _draw():
	if !show_debug: return
	if current_throw_position != Vector2.ZERO:
		# 1. Convert the global collision point to a position relative to THIS node
		var local_target = to_local(current_throw_position)
		
		# 2. Draw the circle at that local spot
		draw_circle(local_target, 12.0, Color(0, 1, 0, 0.5)) 
		
		# 3. Draw a line from the center of THIS node (0,0) to the target
		draw_line(Vector2.ZERO, local_target, Color.WHITE, 2.0)
func get_circle_shot_center(target_global_pos: Vector2, radius: float) -> Vector2:
	var space_state = get_world_2d().direct_space_state
	
	# 1. Define the "Projectile" (The Circle)
	var shape = CircleShape2D.new()
	shape.radius = radius
	
	# 2. Setup the Shot
	var query = PhysicsShapeQueryParameters2D.new()
	query.shape = shape
	query.transform = Transform2D(0, global_position) # Start at Player
	query.motion = target_global_pos - global_position # The "Shot" vector
	#query.exclude = [get_rid()] # Don't hit yourself
	
	# 3. Fire the Shot
	# cast_motion returns [safe_fraction, unsafe_fraction]
	# safe_fraction is a 0.0 to 1.0 multiplier of how far it traveled
	var result = space_state.cast_motion(query)
	var travel_fraction = result[0]
	
	# 4. Calculate the Center
	# If travel_fraction is 1.0, it hit nothing. 
	# If it's 0.5, it hit halfway.
	var final_center = global_position + (query.motion * travel_fraction)
	
	return final_center
func remove_throwable():
	throwable = null
	reset_changing_data()
	
func try_throw() -> bool: 
	if !throwable: return false
	#print("You threw item!")
	throwable.throw(current_throw_position)
	remove_throwable()
	on_throw.emit(self)
	
	return true

func try_start_charge(_throwable: IThrowable) -> bool:
	if is_charging or _throwable == null: return false
	#print("Started charge...")
	throwable = _throwable
	current_charge = 0
	is_charging = true
	target_sprite_node.visible = true
	on_throw_charge_start.emit(self)
	
	return true
	
func cancel_charge():
	reset_changing_data()
	on_throw_charge_cancel.emit(self)
	
func reset_changing_data():
	is_charging = false
	current_charge = 0
	current_throw_position = Vector2.ZERO
	
	update_ui()
	target_sprite_node.visible = false
	
func set_target_direction(direction: Vector2):
	normalized_desired_throw_direction = direction.normalized()

func update_ui():
	target_sprite_node.global_position = current_throw_position
	
func get_normalized_charge() -> float:
	return current_charge / max_charge_seconds
