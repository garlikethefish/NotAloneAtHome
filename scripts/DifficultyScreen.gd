extends Control
var imhere: bool

func _on_hard_button_pressed() -> void:
	GameManager.start(GameManager.GameDifficulty.Hard)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.

func _on_medium_button_pressed() -> void:
	GameManager.start(GameManager.GameDifficulty.Medium)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.

func _on_easy_button_pressed() -> void:
	GameManager.start(GameManager.GameDifficulty.Easy)
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
	pass # Replace with function body.
