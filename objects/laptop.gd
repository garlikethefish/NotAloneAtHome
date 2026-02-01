extends Sprite2D
class_name Laptop

@onready var ui = $ProgrammingMinigame
func _process(delta: float) -> void:
	hide_overlay()


func show_overlay():
	# if press interact, then show this
	GameManager.player_can_move = false
	ui.visible = true
func hide_overlay():
	if Input.is_action_just_pressed("exit"):
		ui.visible = false
		GameManager.player_can_move = true

func _on_programming_minigame_minigame_failed() -> void:
	GameManager.player_can_move = true
	ui.visible = false


func _on_ready() -> void:
	GameManager.laptop = self
