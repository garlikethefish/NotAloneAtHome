extends Node2D

var polygons: Array[Polygon2D] = []

func _draw():
	for poly in polygons:
		# moves them to where parent is (point coords are from 0.0 in they'r space, not world space)
		var global_points = poly.get_global_transform() * poly.polygon
		FogShapes.draw_soft_shape(self, global_points)
