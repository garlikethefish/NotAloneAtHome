extends Polygon2D

func _draw():
	var points: Array[Vector2] = []
	for p in polygon:
		points.append(p)
	print("polygon points: ", points)
	FogShapes.draw_soft_shape(self, points)
