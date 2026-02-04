extends Node2D
class_name DeadThiefCloset
@onready var interactable: IInteractible = $IInteractable

func _on_i_interactable_on_interaction(iInteractor: IInteractor):
	var carrier: ICarrier = Utils.try_get_child_of_type(iInteractor.get_parent(), ICarrier)
	if carrier and carrier.iCariable and Utils.find_parent_of_type(carrier.iCariable, DeadThief):
		hideDeadThief(carrier)
	
func can_interact(_interactor: IInteractor):
	var carrier: ICarrier = Utils.try_get_child_of_type(_interactor.get_parent(), ICarrier)
	if carrier and carrier.iCariable and Utils.find_parent_of_type(carrier.iCariable, DeadThief):
		return true 
	return false 

func hideDeadThief(carrier: ICarrier):
	var deadThief: DeadThief = Utils.find_parent_of_type(carrier.iCariable, DeadThief)
	deadThief.show_into_closet(self)
	carrier.carry_stop()
	GameManager.complete_objective(ObjectiveModel.Objective.HideThief)
