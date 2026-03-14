extends Node2D

class_name ValuableObject

@export var type := ValuableModel.Valuable.None

@onready var helper_holder: HelperHolder  = $HelperHolder
@onready var iSpawnable:    ISpawnable    = helper_holder.get_helper(ISpawnable)
@onready var carriable:     ICarriable    = helper_holder.get_helper(ICarriable)
@onready var interactable:  IInteractible = helper_holder.get_helper(IInteractible)
@onready var destroyable:   IDestroyable  = helper_holder.get_helper(IDestroyable)
@onready var throwable:     IThrowable    = helper_holder.get_helper(IThrowable)
@onready var detectable:    IDetectable   = helper_holder.get_helper(IDetectable)
@onready var sprite := $Sprite2D
var shader_mat := preload("uid://cnuuc1ep5p6ia")

func _ready():
	interactable.can_be_interacted_with_callable = can_be_interacted_with
	carriable.can_be_carried_callable = can_be_carried
	detectable.can_be_detected_callable = can_be_detected
	sprite.texture = GameManager.valuables[type].sprite.texture
	sprite.material = shader_mat


func sell(node: Node2D):
	print("Sold!")
	destroyable.destroy(node)


func can_be_interacted_with(_interactor: IInteractor) -> bool:
	return true


func can_be_detected(_detector: IProximityAreaDetector):
	return true


func can_be_carried(_carrier: ICarrier) -> bool:
	return true


func _on_i_detectable_on_becoming_priority_of_detector(area_detector):
	print("ON")
	interactable.show_sprite()
	sprite.set_instance_shader_parameter("enabled", true)


func _on_i_detectable_on_not_beeing_a_priority_of_detector(area_detector):
	print("off")
	interactable.hide_sprite()
	sprite.set_instance_shader_parameter("enabled", false)
