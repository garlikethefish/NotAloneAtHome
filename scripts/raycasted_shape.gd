extends Node2D

@export var excluded_colliders: Array[CollisionObject2D] = []
@export var reach: float = 150      
@export var start_offset: float = 20
@export var ray_count = 32
@export var collision_masks: Array[int] = [2]
var end_points: Array[Vector2] = []

func _process(_delta):
	end_points.clear()
	var space_state             = get_world_2d().direct_space_state
	var directions              = []
	var excluded_collider_rids  = excluded_colliders.map(func (item: CollisionObject2D): return item.get_rid())
	var combined_mask           = Utils.get_combined_mask(collision_masks)

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
		
		if ray_result:
			end_points.append(to_local(ray_result["position"]))
		else:
			end_points.append(to_local(ray_end))
			
	queue_redraw()


func _draw():
	# This draws the filled shape
	# draw_polygon(points, colors, uvs, texture)
	draw_shape(end_points)
	


func draw_shape(points: Array[Vector2]):
	if points.is_empty(): return
	# Define your edge points as an array of Vector2(x, y)
	var packed_points = PackedVector2Array(points)
	
	draw_polygon(packed_points, [Color.WHITE])
	
	# If you want to draw the outline (to make it look cleaner)
	# draw_polyline(points_plus_closing, color, width, antialiased)
	var outline_points = packed_points
	outline_points.append(packed_points[0]) # Add first point to the end to close the loop
