extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel
@onready var lines_done_count_label = $ImportantInfoPanel/LinesDoneCountLabel
@onready var trash_count_label = $ImportantInfoPanel/TrashCountLabel
@onready var money_lost_count_label = $StolenStuffPanel/MoneyLostCountLabel
@onready var stolen_progress_bar = $StolenStuffPanel/StolenLabel/ProgressBar
@onready var suspicious_progress_bar = $ImportantInfoPanel/SuspiciousLabel/ProgressBar

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	#objective_content_label.text = GameManager.current_objective
	GameManager.connect(
		"on_objective_changed", 
		Callable(self, "new_objective")
	)
	await get_tree().process_frame
	

func update_suspicion_progress():
	pass
	
func update_stolen_progress():
	pass
	
func update_money_lost():
	pass

func new_objective():
	#objective_content_label.text = GameManager.current_objective
	pass
