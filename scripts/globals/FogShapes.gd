extends Node

func draw_soft_shape(node: Node2D, points: Array[Vector2], steps: int = 8, fade_depth: float = 0.15, base_alpha: float = 0.05):
	if points.is_empty(): return
	
	var packed = PackedVector2Array(points)
	
	# fade rings from outside in
	node.draw_polygon(packed, [Color(1, 1, 1, base_alpha)])
	
	var center = Vector2.ZERO
	for p in points:
		center += p
	center /= points.size()
	
	for i in range(steps):
		var t = float(i) / steps
		var shrunk = PackedVector2Array()
		for p in points:
			shrunk.append(p.lerp(center, t * fade_depth))
		node.draw_polygon(shrunk, [Color(1, 1, 1, t)])
