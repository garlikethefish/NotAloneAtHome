# MaskCamera.gd
extends Camera2D

var follow_camera: Camera2D

func _process(_delta):
	if !follow_camera: return
	
	global_position = follow_camera.global_position
	zoom = follow_camera.zoom
