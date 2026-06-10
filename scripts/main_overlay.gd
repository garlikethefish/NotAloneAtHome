extends Control

@onready var objective_content_label = $ObjectivePanel/ObjectiveContentLabel
@onready var stolen_progress_bar = $StolenStuffPanel/StolenLabel/ProgressBar
@onready var suspicious_progress_bar = $ImportantInfoPanel/SuspiciousLabel/ProgressBar
@onready var money_lost_count_label = $StolenStuffPanel/MoneyLostCountLabel

@export var MaskKeyTexture: TextureRect
@export var SprintKeyTexture: TextureRect
@export var SprintToolTip: Panel
@export var MaskToolTip: Panel

var player: Node
var task_name = ReactiveSignal.new("")
var task_step_name = ReactiveSignal.new("")

var _mask_original_y: float
var _sprint_original_y: float

var current_difficulty

func _ready() -> void:
	initialize("Easy") # duct tape fix because shit dont want to initialize otherwise idk why
	GameManager.GameStarted.connect(initialize)
	GameManager.GameEnded.connect(initialize)
	
	GameManager.MoneyChanged.connect(updateMoney)
	GameManager.SuspicionChanged.connect(updateSuspision)
	GameManager.StolenStuffChanged.connect(upadteItemStealed)

	TaskManager.TaskStepNameChanged.connect(task_step_name_changed)
	TaskManager.TaskNameChanged.connect(task_name_changed)

	ReactiveSignal.use_effect(func():
		objective_content_label.text = task_step_name.value
	)

	player = get_tree().get_first_node_in_group("player")

	if player:
		player.CanSprintChanged.connect(func(value):
			if value:
				Animations.Available(SprintToolTip)
			else:
				Animations.Unavailable(SprintToolTip)
		)

		player.CanToggleMaskChanged.connect(func(value):
			if value:
				Animations.Available(MaskToolTip)
			else:
				Animations.Unavailable(MaskToolTip)
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


func initialize(difficulty):
	current_difficulty = difficulty

	objective_content_label.text = ""

	stolen_progress_bar.max_value = GameManager.MaxStealableItems
	stolen_progress_bar.value = GameManager.StolenStuffAmount
	money_lost_count_label.text = str(0)

	suspicious_progress_bar.value = GameManager.Suspicion

func task_step_name_changed(_name: String) -> void:
	task_step_name.value = _name


func task_name_changed(_name: String) -> void:
	task_name.value = _name


func updateSuspision(value: float) -> void:
	suspicious_progress_bar.value = value

func updateLinesDone():
	pass


func finishTrashColection():
	pass


func updateTrashCollected():
	pass

func updateMoney(value: float) -> void:
	money_lost_count_label.text = str(value)

func upadteItemStealed(value: int) -> void:
	stolen_progress_bar.value = value
