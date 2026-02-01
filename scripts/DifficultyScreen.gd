extends Control


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


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
