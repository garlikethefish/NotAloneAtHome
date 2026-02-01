extends CanvasLayer

func _ready():
	$ColorRect/VBoxContainer/Button.pressed.connect(_on_retry_pressed)

func _on_retry_pressed():
	Engine.time_scale = 1.0
	get_tree().reload_current_scene()
