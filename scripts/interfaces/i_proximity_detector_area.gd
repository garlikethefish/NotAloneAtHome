extends Helper

class_name IProximityAreaDetector

signal on_detectable_enter_in_sight(detectable: IDetectable)
signal on_detectable_exit_the_sight(detectable: IDetectable)
signal on_detectable_enter_area(detectable: IDetectable)
signal on_detectable_exit_area(detectable: IDetectable)

var detectables_in_area: Array[DetectableModel] = []
var detect_only_raycastable  := true
var closest_detectable:      IDetectable
var collision_masks: Array[int] = [10, 2]

var is_in_sight_color:   Color = Color.AZURE
var not_in_sight_color:  Color = Color.CRIMSON
var closest_color:       Color = Color.LIME_GREEN

@export var excluded_coliders: Array[CollisionObject2D] = []
@export var show_debug         := true

@onready var collision_shape:  CollisionShape2D = $CollisionShape2D


func _physics_process(_delta: float) -> void:
	if show_debug: queue_redraw()
	validate_detectables()
	var new_priority = get_closest_detectable()
	
	if new_priority != closest_detectable:
		if closest_detectable != null: closest_detectable.remove_as_area_prioriy(self)
		if new_priority != null:       new_priority.set_as_area_prioriy(self)
		
	closest_detectable = new_priority


func _draw():
	if !show_debug: return
	
	if collision_shape.shape is CircleShape2D:
		draw_circle(collision_shape.position, collision_shape.shape.radius, Color8(255, 0, 0, 30)) 
	
	for model: DetectableModel in detectables_in_area:
		var detectable = model.detectable
		var local_target = to_local(detectable.global_position)
		var color: Color = Color.BLACK
		
		if detectable == closest_detectable:
			color = closest_color
		elif model.is_in_line_of_sight:
			color = is_in_sight_color
		else:
			color = not_in_sight_color
		
		draw_circle(local_target, 2.0, color) 
		draw_line(Vector2.ZERO, local_target, color, .5)


func validate_detectables():
	var excluded_collider_rids = excluded_coliders.map(func (colider): return colider.get_rid())
	var space_state = get_world_2d().direct_space_state
	var combined_mask = Utils.get_combined_mask(collision_masks)
	
	for detectable_model in detectables_in_area:
		var query = PhysicsRayQueryParameters2D.create(
			global_position, 
			detectable_model.detectable.global_position,
			combined_mask,
			excluded_collider_rids
		)
		query.hit_from_inside = true
		
		var result: Dictionary = space_state.intersect_ray(query)
		
		# check if detectable is hit
		if result:
			var detectable = result.collider as IDetectable
			
			if detectable and detectable is IDetectable and detectable == detectable_model.detectable and detectable.can_be_detected(self):
				if !detectable_model.is_in_line_of_sight: on_detectable_enter_in_sight.emit(detectable_model.detectable)
				detectable_model.is_in_line_of_sight = true
				continue
				
		if detectable_model.is_in_line_of_sight: on_detectable_exit_the_sight.emit(detectable_model.detectable)
		detectable_model.is_in_line_of_sight = false


func get_closest_detectable() -> IDetectable:
	if detectables_in_area.is_empty(): return null
	
	var new_closest_detectable: IDetectable = null
	
	for detectable_model in detectables_in_area:
		var detectable = detectable_model.detectable
		if !detectable_model.is_in_line_of_sight: continue
		
		if new_closest_detectable == null:
			new_closest_detectable = detectable
			continue
			
		new_closest_detectable = get_closest_node(self, new_closest_detectable, detectable)

	return new_closest_detectable


func get_closest_node(point_node: Node2D, first_node: Node2D, second_node: Node2D) -> Node2D:
	var first_nodes_distance  = point_node.global_position.distance_to(first_node.global_position)
	var second_nodes_distance = point_node.global_position.distance_to(second_node.global_position)
	var distance_diference    = first_nodes_distance - second_nodes_distance
	
	if distance_diference > 0:
		return second_node
	else:
		return first_node


func _on_body_entered(body: Node2D) -> void:
	var detectable: IDetectable = body as IDetectable
	
	if detectable and !detectables_in_area.any(func(item: DetectableModel): return item.detectable == detectable):
		#print("Found detectable!")
		var new_detectable_model = DetectableModel.new()
		new_detectable_model.init(detectable)
		
		detectables_in_area.append(new_detectable_model)
		detectable.enter_area(self)
		on_detectable_enter_area.emit(detectable)


func _on_body_exited(body: Node2D) -> void:
	var detectable: IDetectable = body as IDetectable
	
	if detectable:
		#print("Removed detectable!")
		var index = detectables_in_area.find_custom(func(item: DetectableModel): return item.detectable == detectable)
		if index != -1:
			detectables_in_area.remove_at(index)
			detectable.exit_area(self)
			on_detectable_exit_area.emit(detectable)
