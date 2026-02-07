extends Node

signal on_item_steal()
signal on_trash_collected()
signal spawnTrash()
signal on_suspicion_change()

signal on_max_suspicion()
signal on_max_items_stolen()

signal on_objective_completed(objective: ObjectiveModel.Objective)
signal on_objective_update(objective: ObjectiveModel.Objective)
signal on_start()

enum GameDifficulty { Easy, Medium, Hard }

var chairSprite: Sprite2D
var laptop: Laptop
var player: PlayerCharacter
var sellZone: SellZone

var gameDificulty: GameDifficulty = GameDifficulty.Easy
var trashSpawners: Array[ISpawner] = []
var valuableSpawners: Array[ISpawner] = []

var player_can_move : bool = true
var trashAtHome: int = 0
var areAllTrashCollected: bool = false
var suspicion : float = 0
var suspicionMultiplier := 0
var linesCompleted : int = 0
var current_objective: ObjectiveModel.Objective
var game_objectives := create_game_objectives()

var stolen_stuff_amount : int = 0
var money_lost: int = 0
var valuables : Dictionary[ValuableModel.Valuable, ValuableModel] = {
	ValuableModel.Valuable.TV:     ValuableModel.new(preload("res://sprites/tv.png"), 70),
	ValuableModel.Valuable.Bed:    ValuableModel.new(preload("res://sprites/bed.png"), 40),
	ValuableModel.Valuable.Chair1: ValuableModel.new(preload("res://sprites/chair1.png"), 400),
	ValuableModel.Valuable.Chair2: ValuableModel.new(preload("res://sprites/chair2.png"), 300),
	ValuableModel.Valuable.Closet: ValuableModel.new(preload("res://sprites/closet.png"), 100),
	ValuableModel.Valuable.Sofa:   ValuableModel.new(preload("res://sprites/sofa.png"), 50),
	ValuableModel.Valuable.Table:  ValuableModel.new(preload("res://sprites/table.png"), 50),
	ValuableModel.Valuable.Vase:   ValuableModel.new(preload("res://sprites/vase.png"), 50),
}

var maxStealableItems := 10
var maxTrashAmount := 0

func _process(_delta: float) -> void:
	handle_suspicion(_delta)
	check_if_all_trash_collected()
	
	var playerCarrier: ICarrier = Utils.try_get_child_of_type(player, ICarrier)
	if playerCarrier and playerCarrier.iCariable and playerCarrier.isCarrying and Utils.try_get_parent_of_type(playerCarrier.iCariable, ValuableObject) and sellZone and !sellZone.hasApeared:
		sellZone.apear()
		
	if playerCarrier and !playerCarrier.iCariable and !playerCarrier.isCarrying and sellZone and sellZone.hasApeared:
		sellZone.disapear()
	
func handle_suspicion(timeDelta: float):
	suspicion += timeDelta * suspicionMultiplier
	on_suspicion_change.emit()
	
	if suspicion >= 100:
		on_max_suspicion.emit()
		
func check_if_all_trash_collected():
	if (trashAtHome <= 0 && !areAllTrashCollected):
		areAllTrashCollected = true
		complete_objective(ObjectiveModel.Objective.CleanHome)
		print("Collected all trash")
		
func complete_objective(objective: ObjectiveModel.Objective):
	game_objectives[objective].isCompleted = true
	on_objective_completed.emit(objective)
	
	var tempObjective = current_objective
	# loop to next uncompleted objective
	while game_objectives[current_objective].isCompleted:
		current_objective = game_objectives[current_objective].nextObjective
		
	if tempObjective != current_objective:
		on_objective_update.emit(current_objective)
	
func collectTrash() -> void:
	trashAtHome -= 1
	on_trash_collected.emit()

func start(difficulty: GameDifficulty):
	reset_game_values()
	await get_tree().process_frame
	await get_tree().process_frame
	
	set_difficulty_values(difficulty)
	current_objective = ObjectiveModel.Objective.TakeThiefsMask
	on_start.emit()
	
	spawn_in_trash()
	spawn_in_ValuableModels()
	
func set_difficulty_values(difficulty: GameDifficulty):
	gameDificulty = difficulty
	
	if difficulty == GameDifficulty.Easy:
		suspicionMultiplier = 1
		maxTrashAmount = 3
	if difficulty == GameDifficulty.Medium:
		suspicionMultiplier = 1.25
		maxTrashAmount = 6
		
	if difficulty == GameDifficulty.Hard:
		suspicionMultiplier = 1.5
		maxTrashAmount = 12
		
func reset_game_values():
	trashSpawners = []
	valuableSpawners = []
	trashAtHome = 0
	suspicion = 0
	linesCompleted = 0
	current_objective = ObjectiveModel.Objective.TakeThiefsMask
	game_objectives = create_game_objectives()
	stolen_stuff_amount = 0
	money_lost = 0
	print("Game values reset")

func spawn_in_trash() -> void:
	print("# Start Trash")
	print("Trash from difficulty: ", maxTrashAmount)
	print("Trash from clamp: ", clamp(maxTrashAmount, 0, trashSpawners.size()))
	var howManyTrashWillBeSpawned: int = clamp(maxTrashAmount, 0, trashSpawners.size())
	# copies spawners
	var spawnerPool: Array[ISpawner] = trashSpawners.duplicate()
	print("spawners:", spawnerPool.size())
	print("spawning items: ", howManyTrashWillBeSpawned)
	# shuffles array
	spawnerPool.shuffle()
	
	# spawn at random spawner
	for i in range(howManyTrashWillBeSpawned):
		spawnerPool[i].spawn(spawnerPool[i].packedScene)
		
	areAllTrashCollected = false
	trashAtHome = howManyTrashWillBeSpawned
	emit_signal("spawnTrash")

func spawn_in_ValuableModels():
	print("ValuableModel spawners: ", valuableSpawners.size())
	for spawner in valuableSpawners:
		spawner.spawn(spawner.packedScene)

func create_game_objectives() -> Dictionary[ObjectiveModel.Objective, ObjectiveModel]: 
	return {
		ObjectiveModel.Objective.TakeThiefsMask: ObjectiveModel.new(
			false, 
			"[wave amp=50.0 freq=5.0 connected=1]GRAB THIEFS' MASK[/wave]", 
			ObjectiveModel.Objective.HideThief
		),
		ObjectiveModel.Objective.HideThief: 
			ObjectiveModel.new(
				false, 
				"[wave amp=50.0 freq=5.0 connected=1]HIDE THE DEAD THIEF[/wave]",
				ObjectiveModel.Objective.CleanHome
			),
		ObjectiveModel.Objective.CleanHome: 
			ObjectiveModel.new(
				false, 
				"[wave amp=50.0 freq=5.0 connected=1]CLEAN ROOM[/wave]",
				ObjectiveModel.Objective.FeedKitty
			),
		ObjectiveModel.Objective.FeedKitty:
			ObjectiveModel.new(
				false, 
				"[wave amp=50.0 freq=5.0 connected=1]FEED KITTY[/wave]",
				ObjectiveModel.Objective.WriteCode
			),
		ObjectiveModel.Objective.WriteCode:
			ObjectiveModel.new(
				false, 
				"[wave amp=50.0 freq=5.0 connected=1]WRITE CODE[/wave]",
				ObjectiveModel.Objective.Escape
			),
		ObjectiveModel.Objective.Escape: 
			ObjectiveModel.new(
				false, 
				"[wave amp=50.0 freq=5.0 connected=1]ESCAPE[/wave]",
				ObjectiveModel.Objective.Finish
			)
	}
