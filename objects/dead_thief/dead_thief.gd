extends Node2D

class_name DeadThief

@onready var helper_holder: HelperHolder = $HelperHolder
@onready var interactable: IInteractible = helper_holder.get_helper(IInteractible)
@onready var carriable: ICarriable = helper_holder.get_helper(ICarriable)
@onready var destroyable: IDestroyable = helper_holder.get_helper(IDestroyable)
@onready var detectable: IDetectable = helper_holder.get_helper(IDetectable)


var isHidden := false

func _ready() -> void:
	interactable.can_be_interacted_with_callable = can_be_interacted_with
	carriable.can_be_carried_callable = can_be_carried
	detectable.can_be_detected_callable = can_be_detected


func _on_i_cariable_on_drop(_carrier):
	# enable interactions
	interactable.process_mode = Node.PROCESS_MODE_ALWAYS


func _on_i_cariable_on_pick_up(_carrier):
	# disable interactions
	interactable.process_mode = Node.PROCESS_MODE_DISABLED


func can_be_interacted_with(_interactor: IInteractor) -> bool:
	return GameManager.current_objective == ObjectiveModel.Objective.HideThief


func can_be_carried(_carrier: ICarrier) -> bool:
	if GameManager.current_objective == ObjectiveModel.Objective.HideThief:
		return !isHidden and _carrier and !_carrier.is_carrying
	return false


func can_be_detected(_detector: IProximityAreaDetector) -> bool:
	return true


func show_into_closet(closet: Node2D):
	isHidden = true
	destroyable.destroy(closet)
