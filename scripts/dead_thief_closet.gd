extends Node2D
class_name DeadThiefCloset

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var interactable: IInteractible = helper_holder.get_helper(IInteractible)

func _ready() -> void:
	interactable.can_be_interacted_with_callable = can_be_interacted_with

func _on_i_interactable_on_interaction(interactor: IInteractor):
	var carrier: ICarrier = Utils.try_get_child_of_type(interactor.get_parent(), ICarrier)
	if carrier and carrier.carriable and Utils.find_parent_of_type(carrier.carriable, DeadThief):
		hideDeadThief(carrier)
	
func hideDeadThief(carrier: ICarrier):
	var deadThief: DeadThief = Utils.find_parent_of_type(carrier.carriable, DeadThief)
	deadThief.show_into_closet(self)
	carrier.carry_stop()
	GameManager.complete_objective(ObjectiveModel.Objective.HideThief)

func can_be_interacted_with(_interactor: IInteractor) -> bool:
	var carrier: ICarrier = _interactor.get_helper_from_holder(ICarrier)
	print(carrier, carrier.carriable, is_instance_of(carrier.carriable, DeadThief))
	if carrier and carrier.carriable and is_instance_of(carrier.carriable.helper_holder.main_parent, DeadThief):
		return true 
	return false 
