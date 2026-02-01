extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel
@onready var lines_done_count_label = $ImportantInfoPanel/LinesDoneCountLabel
@onready var trash_count_label = $ImportantInfoPanel/TrashCountLabel
@onready var trash_collected_text_label = $ImportantInfoPanel/TrashCollectedLabel
@onready var money_lost_count_label = $StolenStuffPanel/MoneyLostCountLabel
@onready var stolen_progress_bar = $StolenStuffPanel/StolenLabel/ProgressBar
@onready var suspicious_progress_bar = $ImportantInfoPanel/SuspiciousLabel/ProgressBar

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	objective_content_label.text = GameManager.objective_list[GameManager.current_objective].text
	stolen_progress_bar.max_value = GameManager.maxStealableItems
	trash_count_label.text = "0 / %d" % GameManager.trashAmountFromDifficulty
	stolen_progress_bar.value = 0
	money_lost_count_label.text = "0$"
	GameManager.connect(
		"on_objective_changed", 
		Callable(self, "new_objective")
	)
	GameManager.connect(
		"onItemStealed", 
		Callable(self, "upadteItemStealed")
	)
	GameManager.connect(
		"onTrashCollected",
		Callable(self, "updateTrashCollected")
	)
	GameManager.connect(
		"onAllTrashFinished",
		Callable(self, "finishTrashColection")
	)
	GameManager.connect(
		"onThiefHidden",
		Callable(self, "doneThiefHidden")
	)
	GameManager.connect(
		"onMaskTaken",
		Callable(self, "doneMaskTaken")
	)
	GameManager.connect(
		"onKittyFed",
		Callable(self, "doneFeedingKitty")
	)
	GameManager.connect(
		"onCodeWritten",
		Callable(self, "doneWritingCode")
	)
	GameManager.connect(
		"onSuspicionCnage",
		Callable(self, "updateSuspision")
	)
	
func updateSuspision():
	suspicious_progress_bar.value = GameManager.suspicion
	
func doneThiefHidden():
	GameManager.objective_list[ObjectiveModel.ObjectiveName.HideThief].isCompleted = true
	GameManager.changeObjective()

func doneMaskTaken():
	GameManager.objective_list[ObjectiveModel.ObjectiveName.TakeThiefsMask].isCompleted = true
	GameManager.changeObjective()
	
func doneFeedingKitty():
	GameManager.objective_list[ObjectiveModel.ObjectiveName.FeedKitty].isCompleted = true
	GameManager.changeObjective()
	
func doneWritingCode():
	GameManager.objective_list[ObjectiveModel.ObjectiveName.WriteCode].isCompleted = true
	GameManager.changeObjective()
	
func finishTrashColection():
	trash_collected_text_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	trash_count_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	GameManager.objective_list[ObjectiveModel.ObjectiveName.CleanHome].isCompleted = true
	GameManager.changeObjective()
	
func update_suspicion_progress():
	pass
	
func updateTrashCollected():
	trash_count_label.text = "%d / %d" % [ GameManager.trashAmountFromDifficulty - GameManager.trashAtHome, GameManager.trashAmountFromDifficulty]
	
func upadteItemStealed():
	money_lost_count_label.text = "%d$" % GameManager.money_lost
	stolen_progress_bar.value = GameManager.stolen_stuff_amount
	print("Updated UI")

func new_objective():
	objective_content_label.text = GameManager.objective_list[GameManager.current_objective].text
