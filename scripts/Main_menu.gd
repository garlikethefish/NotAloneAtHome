extends Node
# fuck

func _on_start_button_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/level_1.tscn") #ievada lokciju


func _on_options_button_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/Options.tscn")


func _on_exit_button_pressed() -> void:
	get_tree().quit() #vnk iziet
