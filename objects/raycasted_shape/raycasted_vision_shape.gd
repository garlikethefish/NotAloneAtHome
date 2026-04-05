extends Node2D

@export var excluded_colliders: Array[CollisionObject2D] = []
@export var reach: float = 150      
@export var start_offset: float = 20
@export var ray_count = 32
@export var collision_masks: Array[int] = [2]

var end_points: Array[Vector2] = []

func _process(_delta):
	end_points.clear()
	var space_state = get_world_2d().direct_space_state
	var excluded_collider_rids = excluded_colliders.map(func(item: CollisionObject2D): return item.get_rid())
	var combined_mask = Utils.get_combined_mask(collision_masks)

	for i in range(ray_count):
		var angle = i * (TAU / ray_count)
		var dir = Vector2.RIGHT.rotated(angle)
		var ray_start = global_position + (dir * start_offset)
		var ray_end = global_position + (dir * reach)

		var ray_query = PhysicsRayQueryParameters2D.create(ray_start, ray_end)
		ray_query.exclude = excluded_collider_rids
		ray_query.collision_mask = combined_mask
		ray_query.hit_from_inside = true

		var result = space_state.intersect_ray(ray_query)
		end_points.append(ray_end if not result else result["position"])
