extends Node2D
class_name ObjectSpawner

@export var predifinedValuableSpawner = Valuable.ValuableType.None
var isValuableSpawner: bool:
	get: return predifinedValuableSpawner != Valuable.ValuableType.None
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
	if isObjecSpawned:
		onObjectDestroy()
	
	instantiatedObject = prefab.instantiate()
	instantiatedObject.global_position = self.position
	instantiatedObject.top_level = true
	
	# Adds to current scene
	get_tree().current_scene.add_child(instantiatedObject)
	
	print("Spawning item... ", instantiatedObject)
	print("in pos: ", self.position)
	
	# Cast to InteractableObject safely
	if instantiatedObject is InteractableObject:
		if isValuableSpawner:
			instantiatedObject.objectSprite.texture = GameManager.valuables[predifinedValuableSpawner].sprite.texture
			instantiatedObject.valuable = predifinedValuableSpawner
			instantiatedObject.isCarriable = true
			
			print("sprite: ", GameManager.valuables[predifinedValuableSpawner].sprite)
		else:
			instantiatedObject.objectSprite.texture = GameManager.trashRes
			instantiatedObject.connect(
				"onComplete", 
				Callable(self, "onObjectDestroy")
			)
func onObjectDestroy(): 
	instantiatedObject = null
