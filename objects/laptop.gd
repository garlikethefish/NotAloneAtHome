extends Sprite2D
class_name Laptop

@onready var ui = $ProgrammingMinigame
@onready var interactable = $IInteractable
@onready var vignette = ui.get_child(0)
@onready var waiting_retry := false
@onready var rng = RandomNumberGenerator.new()
@onready var wait_countdown_text = $WaitCountdownText
@onready var wait_countdown_timer = $WaitCountdownTimer
@onready var time_to_wait := 0

func _process(delta: float) -> void:
	hide_overlay()
	if GameManager.player.is_dead:
		ui.visible = false
		GameManager.player_can_move = true
		vignette.visible = false

func show_overlay():
	# if press interact, then show this
	GameManager.player_can_move = false
	ui.visible = true
	vignette.visible = true
func hide_overlay():
	if Input.is_action_just_pressed("exit"):
		ui.visible = false
		GameManager.player_can_move = true
		vignette.visible = false

func _on_programming_minigame_minigame_failed() -> void:
	GameManager.player_can_move = true
	vignette.visible = false
	ui.visible = false
	waiting_retry = true
	interactable.update_can_interact_status()
	time_to_wait = rng.randi_range(10, 20) # wait 10-20 seconds before can interact with laptop/minigame again
	wait_countdown_text.text = str(time_to_wait)
	wait_countdown_text.visible = true
	
	while time_to_wait != 0:
		wait_countdown_timer.start(1)
		await wait_countdown_timer.timeout # wait for timer to go one second forward
	ui.reset_mistakes()
	waiting_retry = false
	wait_countdown_text.visible = false
	interactable.update_can_interact_status()

func _on_ready() -> void:
	GameManager.laptop = self


func _on_i_interactable_on_interaction(iInteractor):
	show_overlay()

func can_interact(_interactor):
	if waiting_retry == false:
		return true


func _on_wait_countdown_timer_timeout() -> void:
	time_to_wait -= 1
	wait_countdown_text.text = str(time_to_wait)
