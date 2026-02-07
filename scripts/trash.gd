extends Node2D
class_name TrashObject

@onready var spawnable: ISpawnable = $ISpawnable
@onready var sprite: Sprite2D = $Sprite2D
@onready var trashRes = preload("res://canvas_textures/trash1_texture.tres")
@export var tapsTillDone := 5

func _ready():
	sprite.texture = trashRes

func _on_i_spawnable_on_spawn(_spawner: ISpawner):
	print("Spawned trash! at: ", global_position)

func can_interact(_interactor: IInteractor):
	return true

func _on_i_interactable_on_interaction(iInteractor: IInteractor):
	tapsTillDone -= 1
	
	if tapsTillDone <= 0:
		GameManager.collectTrash()
		spawnable.on_despawn.emit()
		queue_free()
