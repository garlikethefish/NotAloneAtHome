extends CanvasLayer

# =========================
# CONFIG
# =========================
const MAX_MISTAKES := 3
const LINE_COUNT := 5

# =========================
# NODES
# =========================
@onready var canvas = self
@onready var source_label: RichTextLabel = canvas.get_node("UIImage/ReplyTextLabel")
@onready var publish_button: Button = canvas.get_node("UIImage/PublishButton")

@onready var inputs: Array[LineEdit] = [
	canvas.get_node("UIImage/InputLabel1"),
	canvas.get_node("UIImage/InputLabel2"),
	canvas.get_node("UIImage/InputLabel3"),
	canvas.get_node("UIImage/InputLabel4"),
	canvas.get_node("UIImage/InputLabel5")
]


# =========================
# STATE
# =========================
var source_lines: PackedStringArray

var mistakes: int = 0
var active_line: int = 0


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
		_register_mistake()
		line_edit.text = ""
		line_edit.caret_column = 0
		return


	if new_text.length() == expected.length():
		_advance_line()


# =========================
# LINE PROGRESSION
# =========================
func _advance_line() -> void:
	# Count completed line globally
	GameManager.linesCompleted += 1

	inputs[active_line].editable = false
	active_line += 1

	# All lines finished
	if active_line >= LINE_COUNT:
		publish_button.disabled = false
		publish_button.grab_focus()
		return

	inputs[active_line].editable = true
	inputs[active_line].grab_focus()


# =========================
# MISTAKES
# =========================
func _register_mistake() -> void:
	mistakes += 1


	if mistakes >= MAX_MISTAKES:
		_kick_player()




# =========================
# END STATES
# =========================
func _kick_player() -> void:
	queue_free()


func _on_success() -> void:
	print("Typing minigame completed")


# =========================
# READY
# =========================
func _ready() -> void:
	source_lines = source_label.text.split("\n", false)

	if source_lines.size() < LINE_COUNT:
		push_error("Source text must have at least %d lines" % LINE_COUNT)
		return

	for i in range(inputs.size()):
		var line_edit: LineEdit = inputs[i]
		line_edit.text = ""
		line_edit.context_menu_enabled = false
		line_edit.editable = (i == 0)

		line_edit.text_changed.connect(
			Callable(self, "_on_text_changed").bind(i)
		)

	inputs[0].grab_focus()
	publish_button.disabled = true



func _on_publish_button_pressed() -> void:
	_end_game_success()

func _end_game_success() -> void:
	get_tree().change_scene_to_file("res://scenes/WinScreen.tscn")

	queue_free()
