extends Area2D

func _on_body_entered(body):
	if body.has_method("add_mask"):
		body.add_mask()
		GameManager.complete_objective(ObjectiveModel.Objective.TakeThiefsMask)
		queue_free()
