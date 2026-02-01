extends CanvasLayer

@onready var message_label: Label = $ColorRect/VBoxContainer/Label

func _ready():
	$ColorRect/VBoxContainer/Button.pressed.connect(_on_retry_pressed)

func set_defeat_reason(text: String):
	message_label.text = text

func _on_retry_pressed():
	Engine.time_scale = 1.0
	get_tree().reload_current_scene()
	GameManager.start(GameManager.gameDificulty)
