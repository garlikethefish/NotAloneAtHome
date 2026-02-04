extends Node2D
class_name ISpawner

var spawnedScene: Node
var packedScene: PackedScene

func spawn(scene: PackedScene):
	var instance: Node2D = scene.instantiate()
	get_tree().current_scene.add_child(instance)
	spawnedScene = instance
	instance.process_mode = Node2D.PROCESS_MODE_ALWAYS
	instance.visible = true
	print("Instance created: ", instance)		
	print("class: ", instance.get_script())		
	print("process_mode:", instance.process_mode)
	
	instance.global_position = global_position
	var spawnable: ISpawnable = Utils.try_get_child_of_type(instance, ISpawnable)
	if spawnable != null: spawnable.on_spawn.emit(self)
