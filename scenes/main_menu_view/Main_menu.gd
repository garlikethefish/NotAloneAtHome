extends Node
# fuck

func _on_start_button_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/intro_view/intro.tscn") #ievada lokciju


func _on_options_button_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/options_view/Options.tscn")


func _on_exit_button_pressed() -> void:
	get_tree().quit() #vnk iziet


func _on_fast_start_button_pressed() -> void:
	GameManager.StartGame("easy")
	get_tree().change_scene_to_file("res://scenes/Main.tscn")
