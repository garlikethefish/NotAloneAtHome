extends Node2D
class_name ObjectSpawner

var instantiatedObject: Node2D = null
var isObjecSpawned: bool:
	get: return instantiatedObject != null

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameManager.objectSpawners.append(self)
	pass # Replace with function body.
	
func spawObject(prefab: PackedScene) -> void:
	if isObjecSpawned:
		onObjectDestroy()
	
	instantiatedObject = prefab.instantiate()
	instantiatedObject.global_position = self.position
	
	# Adds to current scene
	get_tree().current_scene.add_child(instantiatedObject)
	
	print("Spawning item... ", instantiatedObject)
	print("in pos: ", self.position)
	# Cast to InteractableObject safely
	if instantiatedObject.is_in_group("DoableObjectGroup"):
		instantiatedObject.connect(
			"onDestroy", 
			Callable(self, "onObjectDestroy")
		)
			
func onObjectDestroy(): 
	instantiatedObject = null
