extends Control
var imhere: bool

func _on_hard_button_pressed() -> void:
	GameManager.StartGame(2)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.

func _on_medium_button_pressed() -> void:
	GameManager.StartGame(1)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.

func _on_easy_button_pressed() -> void:
	GameManager.StartGame(0)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.
