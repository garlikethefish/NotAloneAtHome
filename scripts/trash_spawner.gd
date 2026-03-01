extends Node2D

@export var spawnableScene: PackedScene = preload("res://objects/Trash.tscn")

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var spawner: ISpawner = helper_holder.get_helper(ISpawner)

func _ready():
	spawner.packedScene = spawnableScene
	GameManager.trashSpawners.append(spawner)
