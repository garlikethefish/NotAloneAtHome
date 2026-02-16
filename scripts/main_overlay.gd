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
	GameManager.on_start.connect(initialize)
	GameManager.on_objective_completed.connect(func (objective: ObjectiveModel.Objective) -> void:
		if objective == ObjectiveModel.Objective.CleanHome:
			finishTrashColection()
	)
	GameManager.on_objective_update.connect(func (objective: ObjectiveModel.Objective) -> void:
		objective_content_label.text = GameManager.game_objectives[objective].text
	)
	GameManager.on_trash_collected.connect(updateTrashCollected)
	GameManager.on_suspicion_change.connect(updateSuspision)
	GameManager.on_item_steal.connect(upadteItemStealed)
	GameManager.on_line_completed.connect(updateLinesDone)
	
func initialize():
	objective_content_label.text = GameManager.game_objectives[GameManager.current_objective].text
	stolen_progress_bar.max_value = GameManager.maxStealableItems
	trash_count_label.text = "0 / %d" % GameManager.maxTrashAmount
	lines_done_count_label.text = "0/4"
	stolen_progress_bar.value = 0
	money_lost_count_label.text = "0$"
	
func updateSuspision():
	suspicious_progress_bar.value = GameManager.suspicion
	
func updateLinesDone():
	lines_done_count_label.text = (str(GameManager.linesCompleted) + "/4")
	
func finishTrashColection():
	trash_collected_text_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	trash_count_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	
func updateTrashCollected():
	trash_count_label.text = "%d / %d" % [ 
		GameManager.maxTrashAmount - GameManager.trashAtHome, 
		GameManager.maxTrashAmount
	]
	
func upadteItemStealed():
	money_lost_count_label.text = "%d$" % GameManager.money_lost
	stolen_progress_bar.value = GameManager.stolen_stuff_amount
