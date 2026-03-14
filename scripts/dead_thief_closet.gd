extends Node2D

class_name DeadThiefCloset

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var interactable: IInteractible = helper_holder.get_helper(IInteractible)
@onready var detectable:   IDetectable   = helper_holder.get_helper(IDetectable)

func _ready() -> void:
	interactable.can_be_interacted_with_callable = can_be_interacted_with
	detectable.can_be_detected_callable = can_be_detected

	
func hideDeadThief(carrier: ICarrier):
	print("Hiding dead bodi")
	var deadThief: DeadThief = Utils.find_parent_of_type(carrier.carriable, DeadThief)
	deadThief.show_into_closet(self)
	carrier.try_to_drop()
	GameManager.complete_objective(ObjectiveModel.Objective.HideThief)


func can_be_interacted_with(_interactor: IInteractor) -> bool:
	var carrier: ICarrier = _interactor.get_fellow_helper(ICarrier)
	
	print(carrier.carriable.main_parent is DeadThief)
	if carrier and carrier.carriable and carrier.carriable.main_parent is DeadThief:
		return true 
	return false


func can_be_detected(_detector: IProximityAreaDetector) -> bool:
	var carrier: ICarrier = _detector.get_fellow_helper(ICarrier)
	
	if carrier and carrier.carriable:
		var carriable = carrier.carriable
		if carriable.main_parent is DeadThief:
			return true 
	return false


func _on_i_interactable_on_interaction(_interactor: IInteractor) -> void:
	var carrier: ICarrier = _interactor.get_fellow_helper(ICarrier)
	
	if carrier and carrier.carriable and carrier.carriable.main_parent is DeadThief:
		hideDeadThief(carrier)
