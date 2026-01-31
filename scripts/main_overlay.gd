extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	objective_content_label.text = GameManager.current_objective

func _on_progress_bar_changed() -> void:
	pass # Replace with function body.
