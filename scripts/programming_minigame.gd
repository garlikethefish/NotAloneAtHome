extends CanvasLayer

@onready var input_label_1 = $UIImage/InputLabel1
@onready var input_label_2 = $UIImage/InputLabel2
@onready var input_label_3 = $UIImage/InputLabel3
@onready var input_label_4 = $UIImage/InputLabel4
@onready var input_label_5 = $UIImage/InputLabel5

var current_line : int = 0

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	input_label_1.grab_focus()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("exit"):
		pass # exit UI


func _on_publish_button_pressed() -> void:
	pass # Replace with function body.
