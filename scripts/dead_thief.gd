extends Node2D
class_name DeadThief

@onready var interactable: IInteractible = $IInteractable
@onready var cariable: ICariable = $ICariable
@onready var destroyable: IDestroyable = $IDestroyable
var isHidden := false

func _on_i_cariable_on_drop(_carrier):
	# enable interactions
	interactable.process_mode = Node.PROCESS_MODE_ALWAYS

func _on_i_cariable_on_pick_up(_carrier):
	# disable interactions
	interactable.process_mode = Node.PROCESS_MODE_DISABLED

func can_interact(interactor: IInteractor):
	if GameManager.current_objective == ObjectiveModel.Objective.HideThief:
		var carrier: ICarrier = Utils.try_get_child_of_type(interactor.get_parent(), ICarrier)
		return can_be_carried(carrier)
	
func can_be_carried(_carrier: ICarrier):
	if GameManager.current_objective == ObjectiveModel.Objective.HideThief:
		return !isHidden and _carrier and !_carrier.isCarrying

func show_into_closet(closet: Node2D):
	isHidden = true
	destroyable.destroy(closet)
