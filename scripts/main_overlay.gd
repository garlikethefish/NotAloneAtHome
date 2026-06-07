extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel
@onready var stolen_progress_bar = $StolenStuffPanel/StolenLabel/ProgressBar
@onready var suspicious_progress_bar = $ImportantInfoPanel/SuspiciousLabel/ProgressBar

@export var MaskKeyTexture: TextureRect;
@export var SprintKeyTexture: TextureRect;
@export var SprintToolTip: Panel;
@export var MaskToolTip: Panel;

var player: Node;
var task_name = ReactiveSignal.new("")
var task_step_name = ReactiveSignal.new("")

var _mask_original_y: float
var _sprint_original_y: float

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameManager.GameStarted.connect(initialize)
	TaskManager.TaskStepNameChanged.connect(task_step_name_changed)
	TaskManager.TaskNameChanged.connect(task_name_changed)
	
	ReactiveSignal.use_effect(func():
		objective_content_label.text = task_step_name.value;
	)
	player = get_tree().get_nodes_in_group("player")[0]
	player.CanSprintChanged.connect(func(value):
		if value: Animations.Available(SprintToolTip)
		else: Animations.Unavailable(SprintToolTip)
	)
	player.CanToggleMaskChanged.connect(func(value):
		if value: Animations.Available(MaskToolTip)
		else: Animations.Unavailable(MaskToolTip)
	)
	
	_mask_original_y = MaskKeyTexture.position.y
	_sprint_original_y = SprintKeyTexture.position.y

func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("toggle_mask"):
		Animations.PressDown(MaskKeyTexture, _mask_original_y)
	if Input.is_action_just_released("toggle_mask"):
		Animations.PressUp(MaskKeyTexture, _mask_original_y)
	if Input.is_action_just_pressed("sprint"):
		Animations.PressDown(SprintKeyTexture, _sprint_original_y)
	if Input.is_action_just_released("sprint"):
		Animations.PressUp(SprintKeyTexture, _sprint_original_y)
		
func initialize():
	objective_content_label.text = GameManager.game_objectives[GameManager.current_objective].text
	stolen_progress_bar.max_value = GameManager.maxStealableItems
	#trash_count_label.text = "0 / %d" % GameManager.maxTrashAmount
	#lines_done_count_label.text = "0/4"
	stolen_progress_bar.value = 0
	#money_lost_count_label.text = "0$"
	
func task_step_name_changed(_name: String) -> void:
	task_step_name.value = _name
	
func task_name_changed(_name: String) -> void:
	task_name.value = _name
	
func updateSuspision():
	suspicious_progress_bar.value = GameManager.suspicion
	
func updateLinesDone():
	#lines_done_count_label.text = (str(GameManager.linesCompleted) + "/4")
	pass
	
func finishTrashColection():
	pass
	#trash_collected_text_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	#trash_count_label.add_theme_color_override("font_color", Color(0.609, 0.836, 0.302, .5))
	
func updateTrashCollected():
	pass
	#trash_count_label.text = "%d / %d" % [ 
		#GameManager.maxTrashAmount - GameManager.trashAtHome, 
		#GameManager.maxTrashAmount
	#]
	
func upadteItemStealed():
	#money_lost_count_label.text = "%d$" % GameManager.money_lost
	stolen_progress_bar.value = GameManager.stolen_stuff_amount
