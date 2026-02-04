extends Node2D

@export var spawnableScene: PackedScene = preload("res://objects/Trash.tscn")
@onready var spawner: ISpawner = $ISpawner

func _ready():
	spawner.packedScene = spawnableScene
	GameManager.trashSpawners.append(spawner)
