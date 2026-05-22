namespace NotAloneAtHome.Scripts.Globals;

using Godot;

public enum GameDifficulty { Easy, Medium, Hard }
public enum GameStatus { Started, Paused, Ended }

public partial class GameManager : Node
{
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void GameEndedEventHandler();
    public static GameManager Instance { get; private set; }
    private GameDifficulty _difficulty = GameDifficulty.Easy;
    public GameStatus GameStatus = GameStatus.Ended;
    public float Suspicion = 0;
    public int StolenStuffAmount = 0;
    public float MoneyStolen = 0;
    public int MaxStealableItems = 10;
    public override void _Ready() => Instance = this; 

    public void StartGame(GameDifficulty difficulty)
    {
        GameStatus = GameStatus.Started;
        EmitSignal(SignalName.GameStarted);
    }

    public void EndGame()
    {
        GameStatus = GameStatus.Ended;
        EmitSignal(SignalName.GameEnded);
    }
}


// func start(difficulty: GameDifficulty):
// 	reset_game_values()
// 	await get_tree().process_frame
// 	await get_tree().process_frame
	
// 	set_difficulty_values(difficulty)
// 	current_objective = ObjectiveModel.Objective.TakeThiefsMask
// 	on_start.emit()
	
// 	spawn_in_trash()
// 	spawn_in_ValuableModels()
	
// func set_difficulty_values(difficulty: GameDifficulty):
// 	gameDificulty = difficulty
	
// 	if difficulty == GameDifficulty.Easy:
// 		suspicionMultiplier = 1
// 		maxTrashAmount = 3
// 	if difficulty == GameDifficulty.Medium:
// 		suspicionMultiplier = 1.25
// 		maxTrashAmount = 6
		
// 	if difficulty == GameDifficulty.Hard:
// 		suspicionMultiplier = 1.5
// 		maxTrashAmount = 12
		
// func reset_game_values():
// 	trashSpawners = []
// 	valuableSpawners = []
// 	trashAtHome = 0
// 	suspicion = 0
// 	linesCompleted = 0
// 	current_objective = ObjectiveModel.Objective.TakeThiefsMask
// 	game_objectives = create_game_objectives()
// 	stolen_stuff_amount = 0
// 	money_lost = 0
// 	#print("Game values reset")

// func _ready() -> void:
// 	pass
// 	#AudioServer.set_bus_mute(
// 		#AudioServer.get_bus_index("Master"),
// 		#true
// 	#)
// // 	#print(Engine.get_version_info())
// func _process(_delta: float) -> void:
// 	if game_won == false:
// 		handle_suspicion(_delta)
// 		check_if_all_trash_collected()
		
// 		if player:
// 			var playerCarrier: ICarrier = player.carrier
// 			if playerCarrier and playerCarrier.carriable and playerCarrier.is_carrying and Utils.try_get_parent_of_type(playerCarrier.carriable, ValuableObject) and sellZone and !sellZone.hasApeared:
// 				sellZone.apear()
				
// 			if playerCarrier and !playerCarrier.carriable and !playerCarrier.is_carrying and sellZone and sellZone.hasApeared:
// 				sellZone.disapear()
	
// func handle_suspicion(timeDelta: float):
// 	suspicion += timeDelta * suspicionMultiplier
// 	on_suspicion_change.emit()
	
// 	if suspicion >= 100:
// 		on_max_suspicion.emit()
		
// func check_if_all_trash_collected():
// 	if (trashAtHome <= 0 && !areAllTrashCollected):
// 		areAllTrashCollected = true
// 		complete_objective(ObjectiveModel.Objective.CleanHome)
// 		#print("Collected all trash")
		
// func complete_objective(objective: ObjectiveModel.Objective):
// 	game_objectives[objective].isCompleted = true
// 	on_objective_completed.emit(objective)
	
// 	var tempObjective = current_objective
	
// 	if random_objectives.has("WriteCode"): # remove linear objectives
// 		random_objectives.erase("WriteCode")
// 		random_objectives.erase("HideThief")
// 		random_objectives.erase("TakeThiefsMask")
// 		random_objectives.erase("Escape")
// 		random_objectives.erase("Finish")
	
// 	# objective progression: random looping and linear advancement
// 	match current_objective:
// 		ObjectiveModel.Objective.WriteCode: # pick random task to do that isn't completed
// 			if !random_objectives.is_empty():
// 				var random_pick = random_objectives.pick_random()
// 				current_objective = ObjectiveModel.Objective.get(random_pick)
// 				random_objectives.erase(random_pick)
// 			else:
// 				current_objective = ObjectiveModel.Objective.Escape
// 		ObjectiveModel.Objective.TakeThiefsMask: #first objective
// 			current_objective = game_objectives[current_objective].nextObjective
// 		ObjectiveModel.Objective.HideThief, ObjectiveModel.Objective.FeedKitty, ObjectiveModel.Objective.CleanHome:
// 			current_objective = ObjectiveModel.Objective.WriteCode
// 			GameManager.locked_out = false
// 			game_objectives[current_objective].isCompleted = false # reset writecode objective to incomplete

// 	if tempObjective != current_objective:
// 		on_objective_update.emit(current_objective)
	
// func collectTrash() -> void:
// 	trashAtHome -= 1
// 	on_trash_collected.emit()


// func spawn_in_trash() -> void:
// 	#print("# Start Trash")
// 	#print("Trash from difficulty: ", maxTrashAmount)
// 	#print("Trash from clamp: ", clamp(maxTrashAmount, 0, trashSpawners.size()))
// 	var howManyTrashWillBeSpawned: int = clamp(maxTrashAmount, 0, trashSpawners.size())
// 	# copies spawners
// 	var spawnerPool: Array[ISpawner] = trashSpawners.duplicate()
// 	#print("spawners:", spawnerPool.size())
// 	#print("spawning items: ", howManyTrashWillBeSpawned)
// 	# shuffles array
// 	spawnerPool.shuffle()
	
// 	# spawn at random spawner
// 	for i in range(howManyTrashWillBeSpawned):
// 		spawnerPool[i].spawn(spawnerPool[i].packedScene)
		
// 	areAllTrashCollected = false
// 	trashAtHome = howManyTrashWillBeSpawned
// 	#emit_signal("spawnTrash")

// func spawn_in_ValuableModels():
// 	#print("ValuableModel spawners: ", valuableSpawners.size())
// 	for spawner in valuableSpawners:
// 		spawner.spawn(spawner.packedScene)

// func create_game_objectives() -> Dictionary[ObjectiveModel.Objective, ObjectiveModel]: 
// 	return {
// 		ObjectiveModel.Objective.TakeThiefsMask: ObjectiveModel.new(
// 			false, 
// 			"[wave amp=50.0 freq=5.0 connected=1]GRAB THIEFS' MASK[/wave]", 
// 			ObjectiveModel.Objective.HideThief
// 		),
// 		ObjectiveModel.Objective.HideThief: 
// 			ObjectiveModel.new(
// 				false, 
// 				"[wave amp=50.0 freq=5.0 connected=1]HIDE THE DEAD THIEF[/wave]",
// 				ObjectiveModel.Objective.CleanHome
// 			),
// 		ObjectiveModel.Objective.CleanHome: 
// 			ObjectiveModel.new(
// 				false, 
// 				"[wave amp=50.0 freq=5.0 connected=1]CLEAN ROOM[/wave]",
// 				ObjectiveModel.Objective.FeedKitty
// 			),
// 		ObjectiveModel.Objective.FeedKitty:
// 			ObjectiveModel.new(
// 				false, 
// 				"[wave amp=50.0 freq=5.0 connected=1]FEED KITTY[/wave]",
// 				ObjectiveModel.Objective.WriteCode
// 			),
// 		ObjectiveModel.Objective.WriteCode:
// 			ObjectiveModel.new(
// 				false, 
// 				"[wave amp=50.0 freq=5.0 connected=1]WRITE CODE[/wave]",
// 				ObjectiveModel.Objective.Escape
// 			),
// 		ObjectiveModel.Objective.Escape: 
// 			ObjectiveModel.new(
// 				false, 
// 				"[wave amp=50.0 freq=5.0 connected=1]ESCAPE[/wave]",
// 				ObjectiveModel.Objective.Finish
// 			)
// 	}
