extends Node2D

class_name ValuableObject

@export var type := ValuableModel.Valuable.None
@onready var iSpawnable: ISpawnable = $ISpawnable
@onready var iCariable: ICariable = $ICariable
@onready var interactable: IInteractible = $IInteractable
@onready var destroyable: IDestroyable = $IDestroyable
@onready var sprite := $Sprite2D

func _ready():
	sprite.texture = GameManager.valuables[type].sprite.texture

func _on_i_cariable_on_pick_up(_carrier: ICarrier):
	# disable interactions
	interactable.process_mode = Node.PROCESS_MODE_DISABLED

func _on_i_cariable_on_drop(_carrier):
	# enable interactions
	interactable.process_mode = Node.PROCESS_MODE_ALWAYS
	
func can_interact(interactor: IInteractor) -> bool:
	var carrier: ICarrier = Utils.try_get_child_of_type(interactor.get_parent(), ICarrier)
	return !carrier.isCarrying
	
func can_be_carried(_carrier: ICarrier):
	return !_carrier.isCarrying
	
func sell(node: Node2D):
	print("Sold!")
	destroyable.destroy(node)
