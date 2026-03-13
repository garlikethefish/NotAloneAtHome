extends Node2D

class_name Helper

@onready var helper_holder: HelperHolder = get_parent()
var main_parent: Node2D:
	get(): return helper_holder.main_parent
var is_parent_phisics_object: bool:
	get(): return helper_holder.is_parent_phisics_object
	
	
# Safely give control to other helper by reseting yourself
func retire_unc():
	pass
	

func get_position_tween():
	print(self.get_script().get_global_name())
	return helper_holder.get_shared_position_tween(self)
	
	
func get_sprite_tween():
	return helper_holder.get_shared_sprite_tween(self)


func is_of_type(type: Variant) -> bool:
	return is_instance_of(self, type)

	
func has_helper_in_holder(type: Variant) -> bool:
	return helper_holder.has_helper(type)

	
func get_helper_from_holder(type: Variant) -> Variant:
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
