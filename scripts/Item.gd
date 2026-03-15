extends Area2D

@onready var mask_light := self.get_tree().current_scene.get_node("Lights/MaskLight")


func _on_body_entered(body):
	if body.has_method("add_mask"):
		mask_light.visible = false
		body.add_mask()
		GameManager.complete_objective(ObjectiveModel.Objective.TakeThiefsMask)
		queue_free()
