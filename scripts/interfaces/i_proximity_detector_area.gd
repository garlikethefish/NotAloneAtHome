extends Helper

class_name IProximityAreaDetector

signal on_detectable_detection(detectable: IDetectable)
signal on_detectable_removal(detectable: IDetectable)

## Key: detectable, Value: isnt obstructed by collider
var all_detectables_in_area: Dictionary[IDetectable, bool] = {}
var detect_only_raycastable  := true
var closest_detectable:      IDetectable

var detected_color:   Color = Color.AZURE
var undetected_color: Color = Color.CRIMSON
var closest_color:    Color = Color.LIME_GREEN

@export var excluded_coliders: Array[CollisionObject2D] = []
@export var show_debug         := true
@onready var area:             Area2D = $Area2D
@onready var collision_shape:  CollisionShape2D = $Area2D/CollisionShape2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	closest_detectable = get_closest_detectable()
	
func _physics_process(_delta: float) -> void:
	if show_debug: queue_redraw()
	validate_detectables()
	
func _draw():
	if !show_debug: return
	
	if collision_shape.shape is CircleShape2D:
		draw_circle(collision_shape.position, collision_shape.shape.radius, Color8(255, 0, 0, 30)) 
	
	for detectable in all_detectables_in_area.keys():
		var local_target = to_local(detectable.global_position)
		var color: Color = Color.BLACK
		
		if detectable == closest_detectable:
			color = closest_color
		elif all_detectables_in_area[detectable]:
			color = detected_color
		else:
			color = undetected_color
		
		draw_circle(local_target, 2.0, color) 
		
		# 3. Draw a line from the center of THIS node (0,0) to the target
		draw_line(Vector2.ZERO, local_target, color, .5)
		
func validate_detectables():
	for detectable in all_detectables_in_area.keys():
		var space_state = get_world_2d().direct_space_state
	
		# Setup the query (From, To, Collision Mask)
		var query = PhysicsRayQueryParameters2D.create(
			global_position, 
			detectable.global_position,
			0xFFFFFFFF
		)
		# Execute the rayhelper_holder.main_parent.get_rid(),
		query.exclude = excluded_coliders.map(func (colider):
			return colider.get_rid()
		)
		var result: Dictionary = space_state.intersect_ray(query)
		
		if result:
			#print(result.collider)
			all_detectables_in_area[detectable] = false
		else:
			all_detectables_in_area[detectable] = true


func get_closest_detectable() -> IDetectable:
	if all_detectables_in_area.is_empty(): return null
	
	var new_closest_detectable: IDetectable = null
	
	for detectable in all_detectables_in_area.keys():
		if !all_detectables_in_area[detectable]: continue
		
		if new_closest_detectable == null:
			new_closest_detectable = detectable
			continue
			
		new_closest_detectable = get_closest_node(self, new_closest_detectable, detectable)

	return new_closest_detectable


func get_closest_node(point_node: Node2D, first_node: Node2D, second_node: Node2D) -> Node2D:
	var first_nodes_distance = point_node.global_position.distance_to(first_node.global_position)
	var second_nodes_distance = point_node.global_position.distance_to(second_node.global_position)
	var distance_diference = first_nodes_distance - second_nodes_distance
	if distance_diference > 0:
		return second_node
	else:
		return first_node


func _on_area_2d_area_entered(area: Area2D) -> void:
	var parent = area.get_parent()
	if parent and is_instance_of(parent, IDetectable) and !all_detectables_in_area.has(parent):
		all_detectables_in_area.set(parent, false)


func _on_area_2d_area_exited(area: Area2D) -> void:
	var parent = area.get_parent()
	if parent and is_instance_of(parent, IDetectable) and all_detectables_in_area.has(parent):
		all_detectables_in_area.erase(parent)
