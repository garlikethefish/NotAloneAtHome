extends Node2D
class_name IInteractor

signal on_interactable_change(_iInteractable: IInteractible)
signal on_interactor_status_update()

@onready var interactionArea: Area2D = $Area2D

var interactablesInArea: Array[IInteractible] = []
var iInteractable: IInteractible

func _ready():
	interactionArea.scale = Vector2.ONE * 5

func _process(_delta):
	var closestInteractable = get_closest_interaction()
	
	if iInteractable == closestInteractable: return
	
	if iInteractable:
		iInteractable.clear_interactor()
		
	iInteractable = closestInteractable
	on_interactable_change.emit(iInteractable)
	
	if iInteractable:
		iInteractable.set_interactor(self)
	print("Switched interactable")

func _on_area_2d_area_entered(area: Area2D):
	var interactable = Utils.try_get_parent_of_type(area, IInteractible) as IInteractible
	
	if !interactable: return
	print("Entered Interactable")
	
	if interactablesInArea.has(interactable): return
	interactablesInArea.append(interactable)

func _on_area_2d_area_exited(area):
	var interactable = Utils.try_get_parent_of_type(area, IInteractible) as IInteractible
	
	if !interactable: return
	print("Exited Interactable")
	
	if !interactablesInArea.has(interactable): return
	interactablesInArea.erase(interactable)
	interactable.clear_interactor()
	
func interact():
	if !iInteractable or !can_interact(iInteractable): return
	
	iInteractable.interact(self)
	
func get_closest_interaction() -> IInteractible:
	var closestInteractable: IInteractible = null
	
	if interactablesInArea.size() <= 0:
		iInteractable = null
		return
	
	for interactable in interactablesInArea:
		if closestInteractable == null:
			closestInteractable = interactable
			continue
			
		if self.global_position.distance_to(interactable.global_position) < self.global_position.distance_to(closestInteractable.global_position):
			closestInteractable = interactable
			
	return closestInteractable

func can_interact(_interactable: IInteractible):
	var parent := get_parent()
	return parent != null and parent.has_method("can_interact") and parent.can_interact(_interactable)
