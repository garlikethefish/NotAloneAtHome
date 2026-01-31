extends Node
enum GameDifficulty { Easy, Medium, Hard }
var interactableObjectPrefab: PackedScene = preload("res://objects/InteractableObject.tscn")

var gameDificulty: GameDifficulty = GameDifficulty.Easy
var objectSpawners: Array[ObjectSpawner] = []
var trashAtHome: int = 0
var areAllTrashCollected: bool = false
var suspicion : int = 0
var linesCompleted : int = 0
var current_objective : String = "None"
var objective_list : Array = [
	"GRAB THIEFS' MASK",
	"HIDE THE DEAD THIEF",
	"CLEAN ROOM",
	"FEED KITTY",
	"WRITE CODE",
	"ESCAPE"
]
var current_objective_int : int = 0

var trashAmountFromDifficulty: int:
	get: 
		if gameDificulty == GameDifficulty.Easy:
			return 3
		elif gameDificulty == GameDifficulty.Medium:
			return 5
		elif gameDificulty == GameDifficulty.Hard:
			return 6
		else:
			return 0

signal spawnTrash()

func collectTrash() -> void:
	trashAtHome -= 1
	print("Collected one trash")
	
func addTrash() -> void:
	trashAtHome += 1

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	current_objective = objective_list[0]
	await get_tree().process_frame
	startTrashCollectionTask()

func changeObjective(): # display next objective
	current_objective_int += 1
	current_objective = objective_list[current_objective_int]

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if (trashAtHome <= 0 && !areAllTrashCollected):
		areAllTrashCollected = true
		print("Collected all trash")
		changeObjective()
		
func startTrashCollectionTask() -> void:
	var howManyTrashWillBeSpawned: int = clamp(trashAmountFromDifficulty, 0, objectSpawners.size())
	# copies spawners
	var spawnerPool: Array[ObjectSpawner] = objectSpawners.duplicate()
	print("spawners:", spawnerPool.size())
	print("spawning items: ", howManyTrashWillBeSpawned)
	# shuffles array
	spawnerPool.shuffle()
	
	# spawn at random spawner
	for i in range(howManyTrashWillBeSpawned):
		spawnerPool[i].spawObject(interactableObjectPrefab)
		
	areAllTrashCollected = false
	trashAtHome = howManyTrashWillBeSpawned
	emit_signal("spawnTrash")
