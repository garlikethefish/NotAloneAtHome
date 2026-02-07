extends Node
class_name IDestroyable

signal on_killing_itself()

var tween: Tween
var isKillingItself = false

func destroy(goToNode: Node2D):
	# animation, then destroy
	var parent: Node2D = get_parent()
	isKillingItself = true
	on_killing_itself.emit()
	
	if !tween:
		tween = create_tween()
	else:
		tween.kill()
		tween = create_tween()

	tween.tween_property(parent, "scale", Vector2(0,0), .5)
	tween.set_parallel()
	tween.tween_property(parent, "rotation", 20, .5)
	tween.set_parallel()
	tween.tween_property(parent, "global_position", goToNode.global_position, .2)
	
	tween.finished.connect(func():
		parent.queue_free()
	)
