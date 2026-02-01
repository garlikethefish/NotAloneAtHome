extends Node

signal on_objective_changed()
signal onItemStealed()
signal onTrashCollected()
signal spawnTrash()
signal onSuspicionCnage()

signal onMaxSuspicion()
signal onEnoughItemsStolen()

#quest signals
signal onThiefHidden()
signal onMaskTaken()
signal onKittyFed()
signal onAllTrashFinished()
signal onCodeWritten()
signal onEscape()

enum GameDifficulty { Easy, Medium, Hard }
var interactableObjectPrefab: PackedScene = preload("res://objects/InteractableObject.tscn")

var chairSprite: Sprite2D
var laptop: Laptop

var gameDificulty: GameDifficulty = GameDifficulty.Easy
var objectSpawners: Array[ObjectSpawner] = []
var valuableSpawners: Array[ObjectSpawner] = []

var player_can_move : bool = true
var trashAtHome: int = 0
var areAllTrashCollected: bool = false
var suspicion : float = 0
var suspicionMultiplier := 0
var linesCompleted : int = 0
var current_objective : ObjectiveModel.ObjectiveName = ObjectiveModel.ObjectiveName.TakeThiefsMask
var objectiveDictionaryTemplate : Dictionary[ObjectiveModel.ObjectiveName, ObjectiveModel] = {
	ObjectiveModel.ObjectiveName.TakeThiefsMask: ObjectiveModel.new(
		false, 
		"[wave amp=50.0 freq=5.0 connected=1]GRAB THIEFS' MASK[/wave]", 
		ObjectiveModel.ObjectiveName.HideThief
	),
	ObjectiveModel.ObjectiveName.HideThief: 
		ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]HIDE THE DEAD THIEF[/wave]",
			ObjectiveModel.ObjectiveName.CleanHome
		),
	ObjectiveModel.ObjectiveName.CleanHome: 
		ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]CLEAN ROOM[/wave]",
			ObjectiveModel.ObjectiveName.FeedKitty
		),
	ObjectiveModel.ObjectiveName.FeedKitty:
		ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]FEED KITTY[/wave]",
			ObjectiveModel.ObjectiveName.WriteCode
		),
	ObjectiveModel.ObjectiveName.WriteCode:
		ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]WRITE CODE[/wave]",
			ObjectiveModel.ObjectiveName.Escape
		),
	ObjectiveModel.ObjectiveName.Escape: 
		ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]ESCAPE[/wave]",
			ObjectiveModel.ObjectiveName.Finish
		),
}

var objective_list = objectiveDictionaryTemplate.duplicate_deep()
var current_objective_int : int = 0
var stolen_stuff_amount : int = 0
var money_lost: int = 0
var trashRes = preload("res://sprites/trash.png")
var valuables : Dictionary[Valuable.ValuableType, Valuable] = {
	Valuable.ValuableType.TV: Valuable.new(preload("res://sprites/tv.png"), 70),
	Valuable.ValuableType.Bed: Valuable.new(preload("res://sprites/bed.png"), 40),
	Valuable.ValuableType.Chair1: Valuable.new(preload("res://sprites/chair1.png"), 400),
	Valuable.ValuableType.Chair2: Valuable.new(preload("res://sprites/chair2.png"), 300),
	Valuable.ValuableType.Closet: Valuable.new(preload("res://sprites/closet.png"), 100),
	Valuable.ValuableType.Sofa: Valuable.new(preload("res://sprites/sofa.png"), 50),
	Valuable.ValuableType.Table: Valuable.new(preload("res://sprites/table.png"), 50),
	Valuable.ValuableType.Vase: Valuable.new(preload("res://sprites/vase.png"), 50),
}

var maxStealableItems := 10
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

func collectTrash() -> void:
	trashAtHome -= 1
	onTrashCollected.emit()
	print("Collected one trash")
	
func addTrash() -> void:
	trashAtHome += 1

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	suspicion += _delta * suspicionMultiplier
	onSuspicionCnage.emit()
	#print("sus: ", suspicion)
	
	if suspicion >= 100:
		onMaxSuspicion.emit()
		
	if (trashAtHome <= 0 && !areAllTrashCollected):
		areAllTrashCollected = true
		print("Collected all trash")
		onAllTrashFinished.emit()
		changeObjective()
		
func start(difficulty: GameDifficulty):
	clearAll()
	await get_tree().process_frame
	await get_tree().process_frame
	
	await get_tree().process_frame
	await get_tree().process_frame
	await get_tree().process_frame
	await get_tree().process_frame
	await get_tree().process_frame
	
	gameDificulty = difficulty
	
	if difficulty == GameDifficulty.Easy:
		suspicionMultiplier = 1

	if difficulty == GameDifficulty.Medium:
		suspicionMultiplier = 2
		
	if difficulty == GameDifficulty.Hard:
		suspicionMultiplier = 3
	
	startTrashCollectionTask()
	spawnInValuables()
	
	print("Started again!")
	
func clearAll():
	objectSpawners = []
	valuableSpawners = []
	trashAtHome = 0
	suspicion = 0
	linesCompleted = 0
	current_objective = ObjectiveModel.ObjectiveName.TakeThiefsMask
	objective_list = objectiveDictionaryTemplate.duplicate_deep()
	
func changeObjective(): # display next objective
	# go to next uncompleted
	if objective_list[current_objective].isCompleted:
		while objective_list[current_objective].isCompleted:
			current_objective = objective_list[current_objective].nextObjective
	on_objective_changed.emit()
		
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
		spawnerPool[i].spawObject(interactableObjectPrefab, true)
		
	areAllTrashCollected = false
	trashAtHome = howManyTrashWillBeSpawned
	emit_signal("spawnTrash")
	
func spawnInValuables():
	print("valuable spawners: ", valuableSpawners.size())
	for spawner in valuableSpawners:
		spawner.spawObject(interactableObjectPrefab, true)
		
	
