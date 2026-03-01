extends Node2D

class_name ValuableObject

@export var type := ValuableModel.Valuable.None

@onready var helper_holder: HelperHolder  = $HelperHolder
@onready var iSpawnable:    ISpawnable    = helper_holder.get_helper(ISpawnable)
@onready var carriable:     ICarriable    = helper_holder.get_helper(ICarriable)
@onready var interactable:  IInteractible = helper_holder.get_helper(IInteractible)
@onready var destroyable:   IDestroyable  = helper_holder.get_helper(IDestroyable)
@onready var throwable:     IThrowable    = helper_holder.get_helper(IThrowable)
@onready var sprite := $Sprite2D

func _ready():
	interactable.can_be_interacted_with_callable = can_be_interacted_with
	carriable.can_be_carried_callable = can_be_carried
	sprite.texture = GameManager.valuables[type].sprite.texture

func _on_i_cariable_on_pick_up(_carrier: ICarrier):
	# disable interactions
	interactable.process_mode = Node.PROCESS_MODE_DISABLED

func _on_i_cariable_on_drop(_carrier):
	# enable interactions
	interactable.process_mode = Node.PROCESS_MODE_ALWAYS
	
func _on_i_interactable_on_interaction(_interactor: IInteractor) -> void:
	var carrier: ICarrier = _interactor.get_helper_from_holder(ICarrier)
	if carrier: 
		carriable.pick_up(carrier)
	
func can_be_interacted_with(_interactor: IInteractor) -> bool:
	return !throwable.is_flying
	
func can_be_carried(_carrier: ICarrier) -> bool:
	return true
	
func sell(node: Node2D):
	print("Sold!")
	destroyable.destroy(node)
