extends Node

class_name HelperHolder

@onready var main_parent:  Node2D = get_parent()
var shared_position_tween: Tween
var shared_sprite_tween:   Tween 

func get_shared_position_tween(target_node: Node) -> Tween:
	# 1. Kill the old one so the previous script loses control
	if is_instance_valid(shared_position_tween) and shared_position_tween.is_valid():
		shared_position_tween.kill()
	
	shared_position_tween = target_node.create_tween()
	return shared_position_tween
	
func get_shared_sprite_tween(target_node: Node) -> Tween:
	# 1. Kill the old one so the previous script loses control
	if is_instance_valid(shared_sprite_tween) and shared_sprite_tween.is_valid():
		shared_sprite_tween.kill()
	
	shared_sprite_tween = target_node.create_tween()
	return shared_sprite_tween

func _ready() -> void:
	# shoot after _ready
	await main_parent.ready 
	assert_helper_assigned_callables()

func has_helper(type: Variant) -> bool:
	for child in get_children():
		if is_instance_of(child, type):
			return true
	return false

func get_helper(type: Variant) -> Variant:
	for child in get_children():
		if is_instance_of(child, type):
			return child
	return null
	
func assert_helper_assigned_callables():
	for helper: Helper in get_children():
		helper.assert_assigned_callables()
