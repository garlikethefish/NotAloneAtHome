extends Node
class_name IDestroyable

var tween: Tween

func destroy(goToNode: Node2D):
	# animation, then destroy
	var parent = get_parent()
	
	if !tween:
		tween = create_tween()
	else:
		tween.kill()
		tween = create_tween()

	var endPos = goToNode.global_position
	tween.tween_property(parent, "scale", Vector2(0,0), .5)
	tween.set_parallel()
	tween.tween_property(parent, "rotation", 20, .5)
	tween.set_parallel()
	tween.tween_property(parent, "global_position", endPos, .5)
	
	tween.finished.connect(func():
		parent.queue_free()
	)
