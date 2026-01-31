extends Node2D
class_name ObjectSpawner

var predifinedValuableSpawner = Valuable.ValuableType.None
var instantiatedObject: Node2D = null
var isObjecSpawned: bool:
	get: return instantiatedObject != null

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if predifinedValuableSpawner != Valuable.ValuableType.None:
		GameManager.valuableSpawners.append(self)
	else:
		GameManager.objectSpawners.append(self)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
	
func spawObject(prefab: PackedScene, isCarriable: bool) -> void:
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
