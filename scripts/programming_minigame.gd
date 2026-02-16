extends CanvasLayer

signal kick

@onready var canvas = self
@onready var source_label: RichTextLabel = $UIImage/ReplyTextLabel
@onready var publish_button: Button = $UIImage/PublishButton
@onready var mistakes_text : RichTextLabel = $UIImage/MistakesText

@onready var inputs: Array[LineEdit] = [
	canvas.get_node("UIImage/InputLabel1"),
	canvas.get_node("UIImage/InputLabel2"),
	canvas.get_node("UIImage/InputLabel3"),
	canvas.get_node("UIImage/InputLabel4")
]
@onready var indicators = [
	canvas.get_node("UIImage/Indicators/Light0"),
	canvas.get_node("UIImage/Indicators/Light1"),
	canvas.get_node("UIImage/Indicators/Light2"),
	canvas.get_node("UIImage/Indicators/Light3")
]
@onready var inactive_light := preload("res://sprites/inactive_light.png")
@onready var active_light := preload("res://sprites/light.png")

var source_lines: PackedStringArray

var mistakes: int = 0
var active_line: int = 0
@onready var waiting := false


# =========================
# READY
# =========================
func _ready() -> void:
	source_lines = source_label.text.split("\n", false)

	if source_lines.size() < GameManager.max_lines:
		push_error("Source text must have at least %d lines" % GameManager.max_lines)
		return

	for i in range(inputs.size()):
		var line_edit: LineEdit = inputs[i]
		line_edit.text = ""
		line_edit.context_menu_enabled = false
		line_edit.editable = (i == 0)

		line_edit.text_changed.connect(
			Callable(self, "_on_text_changed").bind(i)
		)
	indicators[0].texture = active_light
	indicators[0].get_child(0).visible = true
	inputs[0].grab_focus()
	publish_button.disabled = true

# =========================
# TEXT CHECKING
# =========================
func _on_text_changed(new_text: String, line_index: int) -> void:
	var line_edit: LineEdit = inputs[line_index]

	if line_index != active_line:
		line_edit.text = ""
		return

	var expected: String = source_lines[line_index]

	# Allow backspace
	if new_text.length() < line_edit.text.length():
		return

	# Prevent overflow
	if new_text.length() > expected.length():
		line_edit.text = new_text.substr(0, expected.length())
		return

	var char_index := new_text.length() - 1
	if char_index < 0:
		return

	if new_text[char_index] != expected[char_index]:
		line_edit.mouse_filter = Control.MOUSE_FILTER_IGNORE
		line_edit.focus_mode = Control.FOCUS_NONE
		await get_tree().create_timer(0.2).timeout 
		line_edit.mouse_filter = Control.MOUSE_FILTER_PASS
		line_edit.focus_mode = Control.FOCUS_ALL
		line_edit.grab_focus()
		_register_mistake()
		line_edit.delete_char_at_caret()
		return


	if new_text.length() == expected.length():
		_advance_line()

# =========================
# LINE PROGRESSION
# =========================
func _advance_line() -> void:
	inputs[active_line].editable = false
	active_line += 1
	
	# Count completed line globally
	GameManager.linesCompleted += 1
	GameManager.on_line_completed.emit()
	

	# All lines finished
	if active_line >= GameManager.max_lines:
		publish_button.disabled = false
		publish_button.grab_focus()
		return
		
	# turn off all indicators
	for indicator in indicators:
		indicator.texture = inactive_light
		indicator.get_child(0).visible = false
	
	GameManager.complete_objective(ObjectiveModel.Objective.WriteCode)
	
	_kick_player(false)


# =========================
# MISTAKES
# =========================
func _register_mistake() -> void:
	mistakes += 1
	mistakes_text.text = str("Mistakes ", mistakes, "/3")

	if mistakes >= GameManager.max_mistakes:
		await get_tree().create_timer(0.2).timeout 
		_kick_player(true)

func reset_mistakes():
	mistakes = 0
	mistakes_text.text = str("Mistakes ", mistakes, "/3")


# =========================
# END STATES
# =========================
func _kick_player(fumbled: bool) -> void:
	emit_signal("kick", fumbled)
	


func _on_success() -> void:
	print("Typing minigame completed")
	
	



func _on_publish_button_pressed() -> void:
	GameManager.game_won = true
	_end_game_success()

func _end_game_success() -> void:
	get_tree().change_scene_to_file("res://scenes/WinScreen.tscn")

# =========================
# START NEW LINE
# =========================

func _on_laptop_start_a_new_line() -> void:
	inputs[active_line].editable = true
	inputs[active_line].grab_focus()
	
	# turn on active lines' indicator and turn off rest
	for indicator in indicators:
		if indicator.name.ends_with(str(active_line)) != true:
			indicator.texture = inactive_light
			indicator.get_child(0).visible = false
	indicators[active_line].texture = active_light
	indicators[active_line].get_child(0).visible = true
