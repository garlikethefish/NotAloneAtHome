extends Helper

class_name IInteractor

signal on_interaction(_interactable: IInteractible)
signal on_interactable_change(_interactable: IInteractible)
#signal on_interactor_status_update()

var can_interact_callable: Callable

@onready var interactionArea: Area2D = $Area2D

var interactablesInArea: Array[IInteractible] = []
var selected_interactable: IInteractible

func _ready():
	interactionArea.scale = Vector2.ONE * 5


func _process(_delta):
	var closestInteractable = get_closest_interaction()
	
	if selected_interactable == closestInteractable: return
	
	if selected_interactable:
		selected_interactable.clear_interactor()
		
	selected_interactable = closestInteractable
	on_interactable_change.emit(selected_interactable)
	
	if selected_interactable:
		selected_interactable.set_interactor(self)
	#print("Switched interactable")

func _on_area_2d_area_entered(area: Area2D):
	var interactable = Utils.try_get_parent_of_type(area, IInteractible) as IInteractible
	
	if !interactable: return
	#print("Entered Interactable")
	
	if interactablesInArea.has(interactable): return
	interactablesInArea.append(interactable)

func _on_area_2d_area_exited(area):
	var interactable = Utils.try_get_parent_of_type(area, IInteractible) as IInteractible
	
	if !interactable: return
	#print("Exited Interactable")
	
	if !interactablesInArea.has(interactable): return
	interactablesInArea.erase(interactable)
	interactable.clear_interactor()
	
func interact() -> IInteractible:
	if !selected_interactable or !can_interact(): return null
	
	on_interaction.emit(selected_interactable)
	
	selected_interactable.interact(self)
	return selected_interactable
	
func get_closest_interaction() -> IInteractible:
	var closestInteractable: IInteractible = null
	
	if interactablesInArea.size() <= 0:
		selected_interactable = null
		return
	
	for interactable in interactablesInArea:
		if closestInteractable == null:
			closestInteractable = interactable
			continue
			
		if self.global_position.distance_to(interactable.global_position) < self.global_position.distance_to(closestInteractable.global_position):
			closestInteractable = interactable
			
	return closestInteractable

func can_interact(_interactable: IInteractible = selected_interactable) -> bool:
	return can_interact_callable.call(_interactable)
	
func assert_assigned_callables():
	assert_callable(can_interact_callable, "can_interact_callable")
