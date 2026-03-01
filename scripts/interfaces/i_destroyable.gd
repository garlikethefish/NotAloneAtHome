extends Helper

class_name IDestroyable

signal on_killing_itself()

var isKillingItself = false

func destroy(goToNode: Node2D):
	# animation, then destroy
	var parent: Node2D = helper_holder.main_parent
	isKillingItself = true
	on_killing_itself.emit()
	
	var position_tween = get_position_tween()

	position_tween.tween_property(parent, "scale", Vector2(0,0), .5)
	position_tween.set_parallel()
	position_tween.tween_property(parent, "rotation", 20, .5)
	position_tween.set_parallel()
	position_tween.tween_property(parent, "global_position", goToNode.global_position, .2)
	
	position_tween.finished.connect(func():
		parent.queue_free()
	)
