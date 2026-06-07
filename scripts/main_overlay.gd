extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel
@onready var lines_done_count_label = $ImportantInfoPanel/LinesDoneCountLabel
@onready var trash_count_label = $ImportantInfoPanel/TrashCountLabel
@onready var trash_collected_text_label = $ImportantInfoPanel/TrashCollectedLabel
@onready var money_lost_count_label = $StolenStuffPanel/MoneyLostCountLabel
@onready var stolen_progress_bar = $StolenStuffPanel/StolenLabel/ProgressBar
@onready var suspicious_progress_bar = $ImportantInfoPanel/SuspiciousLabel/ProgressBar
@export var MaskKeyTexture: TextureRect;
@export var SprintKeyTexture: TextureRect;

var task_name = ReactiveSignal.new("")
var task_step_name = ReactiveSignal.new("")

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameManager.GameStarted.connect(initialize)
	TaskManager.TaskStepNameChanged.connect(task_step_name_changed)
	TaskManager.TaskNameChanged.connect(task_name_changed)
	
	ReactiveSignal.use_effect(func():
		objective_content_label.text = task_step_name.value;
	)
	
	_mask_original_y = MaskKeyTexture.position.y
	_sprint_original_y = SprintKeyTexture.position.y
	
var _mask_original_y: float
var _sprint_original_y: float

func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("toggle_mask"):
		press_down(MaskKeyTexture, _mask_original_y)
	if Input.is_action_just_released("toggle_mask"):
		press_up(MaskKeyTexture, _mask_original_y)
	if Input.is_action_just_pressed("sprint"):
		press_down(SprintKeyTexture, _sprint_original_y)
	if Input.is_action_just_released("sprint"):
		press_up(SprintKeyTexture, _sprint_original_y)

func press_down(control: Control, original_y: float) -> void:
	var tween = control.create_tween()
	tween.tween_property(control, "position:y", original_y + 5, 0.08)\
		.set_trans(Tween.TRANS_SINE)

func press_up(control: Control, original_y: float) -> void:
	var tween = control.create_tween()
	tween.tween_property(control, "position:y", original_y, 0.08)\
		.set_trans(Tween.TRANS_SINE)
		
func initialize():
	objective_content_label.text = GameManager.game_objectives[GameManager.current_objective].text
	stolen_progress_bar.max_value = GameManager.maxStealableItems
	trash_count_label.text = "0 / %d" % GameManager.maxTrashAmount
	lines_done_count_label.text = "0/4"
	stolen_progress_bar.value = 0
	money_lost_count_label.text = "0$"
	
func task_step_name_changed(_name: String) -> void:
	task_step_name.value = _name
	
func task_name_changed(_name: String) -> void:
	task_name.value = _name
	
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
