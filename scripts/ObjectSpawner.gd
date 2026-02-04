extends Node2D
class_name ObjectSpawner

@export var spawnableObject: PackedScene = preload("res://objects/Trash.tscn")

@export var predifinedValuableSpawner = ValuableModel.Valuable.None
var isValuableSpawner: bool:
	get: return predifinedValuableSpawner != ValuableModel.Valuable.None
var instantiatedObject: Node2D = null
var isObjecSpawned: bool:
	get: return instantiatedObject != null

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if isValuableSpawner:
		GameManager.valuableSpawners.append(self)
	else:
		GameManager.objectSpawners.append(self)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass
	
func spawObject(prefab: PackedScene, isCarriable: bool) -> void:
	var instance = spawnableObject.instantiate()
	get_tree().current_scene.add_child(instance)
	if instance == null:
		push_error("Failed to instantiate scene")
		return
	if not instance is Node2D:
		push_error("Instantiated root is not Node2D")
		return
	instance = instance as Node2D
	instance.global_position = global_position
	if instance.has_node("ISpawnable"):
		instance.get_node("ISpawnable").on_spawn.emit(self)
		
func onObjectDestroy(): 
	instantiatedObject = null
