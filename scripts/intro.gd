extends Control

@onready var s1 = $Story1
@onready var s2 = $Story2
@onready var s3 = $Story3
@onready var s4 = $Story4
@onready var s5 = $Story5

var current_img_int = 1

func next_img():
	if current_img_int == 2:
		s2.visible = true
	elif current_img_int == 3:
		s3.visible = true
	elif current_img_int == 4:
		s4.visible = true
	elif current_img_int == 5:
		s5.visible = true
	elif current_img_int == 6:
		get_tree().change_scene_to_file("res://scenes/Difficulty.tscn")
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("press"):
		current_img_int += 1
		next_img()
