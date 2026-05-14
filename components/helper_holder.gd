extends Node

class_name HelperHolder

@onready var main_parent: Node2D = get_parent()


func _ready() -> void:
	# shoot after _ready
	await main_parent.ready 
	assert_helper_assigned_callables()

## Checks if such helper is in holder
func has_helper(type: Variant) -> bool:
	for child in get_children():
		if is_instance_of(child, type):
			return true
	return false

## Returns existing helper or null
func get_helper(type: Variant) -> Variant:
	for child in get_children():
		if is_instance_of(child, type):
			return child
	return null

## Makes sure required callables are assigned, before game starts
func assert_helper_assigned_callables():
	for helper: Helper in get_children():
		helper.assert_assigned_callables()
