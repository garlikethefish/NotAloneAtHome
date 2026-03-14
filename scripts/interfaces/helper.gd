extends Node2D

class_name Helper

@onready var helper_holder: HelperHolder = get_parent()

var main_parent: Node2D:
	get(): return helper_holder.main_parent

# Safely give control to other helper by reseting yourself
func retire_unc():
	pass


func is_of_type(type: Variant) -> bool:
	return is_instance_of(self, type)


func has_helper_in_holder(type: Variant) -> bool:
	return helper_holder.has_helper(type)


func get_fellow_helper(type: Variant) -> Variant:
	return helper_holder.get_helper(type)


func assert_callable(callable: Callable, _name: String = "_"):
	if !callable.is_valid():
		push_error("callable " + _name + " not assigned! \nIn: " + helper_holder.main_parent.name + " " + get_script().get_global_name() + "\nIn parent: " + helper_holder.main_parent.get_script().get_global_name())


func assert_callables(callables: Array[Callable]):
	for callable in callables:
		assert_callable(callable)


func assert_assigned_callables() -> void:
	# Optional: common logic for all helpers
	pass
