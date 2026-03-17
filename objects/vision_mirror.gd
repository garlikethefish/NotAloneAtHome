# VisionMirror.gd
extends Node2D

var source: Node2D

func _process(_delta):
	if not source: return
	queue_redraw()


func _draw():
	if not source or source.end_points.is_empty(): return
	FogShapes.draw_soft_shape(self, source.end_points)
