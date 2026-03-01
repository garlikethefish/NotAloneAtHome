extends Node2D
class_name TrashObject

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var spawnable:    ISpawnable    = helper_holder.get_helper(ISpawnable)
@onready var interactable: IInteractible = helper_holder.get_helper(IInteractible)

@onready var sprite: Sprite2D = $Sprite2D
@onready var trashRes = preload("res://canvas_textures/trash1_texture.tres")
@export var tapsTillDone := 5

func _ready():
	sprite.texture = trashRes
	interactable.can_be_interacted_with_callable = can_be_interacted_with

func _on_i_spawnable_on_spawn(_spawner: ISpawner):
	print("Spawned trash! at: ", global_position)

func can_be_interacted_with(_interactor: IInteractor):
	if GameManager.current_objective == ObjectiveModel.Objective.CleanHome:
		return true
	return false
	
func _on_i_interactable_on_interaction(_interactor: IInteractor):
	tapsTillDone -= 1
	
	if tapsTillDone <= 0:
		GameManager.collectTrash()
		spawnable.on_despawn.emit()
		queue_free()
