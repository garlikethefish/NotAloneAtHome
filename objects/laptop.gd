extends Sprite2D
class_name Laptop

signal start_a_new_line

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var interactable: IInteractible = helper_holder.get_helper(IInteractible)
@onready var detectable: IDetectable     = helper_holder.get_helper(IDetectable)

@onready var ui = $ProgrammingMinigame
@onready var vignette = ui.get_child(0)
@onready var waiting_retry := false
@onready var rng = RandomNumberGenerator.new()
@onready var wait_countdown_text = $WaitCountdownText
@onready var wait_countdown_timer = $WaitCountdownTimer
@onready var time_to_wait := 0

func _ready() -> void:
	interactable.can_be_interacted_with_callable = can_be_interacted_with
	detectable.can_be_detected_callable = can_be_detected

func _process(_delta: float) -> void:
	hide_overlay()
	if GameManager.player and GameManager.player.is_dead:
		ui.visible = false
		GameManager.player_can_move = true
		vignette.visible = false

func show_overlay():
	# if press interact, then show this
	GameManager.player_can_move = false
	ui.visible = true
	vignette.visible = true
	start_a_new_line.emit()
func hide_overlay():
	if Input.is_action_just_pressed("exit"):
		ui.visible = false
		GameManager.player_can_move = true
		vignette.visible = false

func _on_ready() -> void:
	GameManager.laptop = self

func _on_i_interactable_on_interaction(_interactor):
	show_overlay()


func can_be_interacted_with(_interactor: IInteractor) -> bool:
	if waiting_retry == false and GameManager.locked_out == false:
		if GameManager.current_objective == ObjectiveModel.Objective.WriteCode:
			print("INTERAAA")
			return true
	return false


func can_be_detected(_detector: IProximityAreaDetector):
	return true


func _on_wait_countdown_timer_timeout() -> void:
	time_to_wait -= 1
	wait_countdown_text.text = str(time_to_wait)


func _on_programming_minigame_kick(fumbled: bool) -> void:
	if fumbled: # too many typing mistakes made
		GameManager.player_can_move = true
		vignette.visible = false
		ui.visible = false
		waiting_retry = true
		time_to_wait = rng.randi_range(10, 20) # wait 10-20 seconds before can interact with laptop/minigame again
		wait_countdown_text.text = str(time_to_wait)
		wait_countdown_text.visible = true
		
		while time_to_wait != 0:
			wait_countdown_timer.start(1)
			await wait_countdown_timer.timeout # wait for timer to go one second forward
		ui.reset_mistakes()
		waiting_retry = false
		wait_countdown_text.visible = false
		# interactable.update_can_interact_status()
	else: # if just advancing to next line objective
		GameManager.player_can_move = true
		vignette.visible = false
		ui.visible = false
		GameManager.locked_out = true
