extends Node2D

@export var spawnableScene: PackedScene = preload("res://objects/valuables/Valuable.tscn")
@onready var spawner: ISpawner = $ISpawner

func _ready():
	spawner.packedScene = spawnableScene
	GameManager.valuableSpawners.append(spawner)
