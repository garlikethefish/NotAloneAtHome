extends Node2D

class_name EscapeArea

@onready var startingSize = scale
var tween: Tween

func _ready():
	scale = Vector2(0,0)
	GameManager.on_objective_completed.connect(func (objective: ObjectiveModel.Objective) -> void:
		if objective == ObjectiveModel.Objective.WriteCode:
			appear()
	)

func _on_area_2d_body_entered(body: Node2D):
	var player = body.get_script()
	
	if is_instance_of(player, PlayerCharacter) and canEscape():
		GameManager.complete_objective(ObjectiveModel.Objective.Escape)

func canEscape():
	return GameManager.game_objectives[ObjectiveModel.Objective.WriteCode].isCompleted
	
func appear():
	if tween: tween.kill()
	
	tween = create_tween()
	tween.tween_property(self, "scale", startingSize, .5).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
