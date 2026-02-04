extends Node2D
class_name InteractableObject


@onready var iInteractable: IInteractible = $IInteractable

@export var interactionTexture: Texture2D
@export var tapsTillDone: int = 10

var allowInteraction: bool
var isKillingItself: bool

# Called when the node enters the scene tree for the first time.
#func _ready() -> void:
	#interactionSprite.texture = interactionSprite.texture
	#interactionSprite.visible = false

#func _on_area_2d_body_entered(body: Node2D) -> void:
	#var interactor = Utils.try_get_child_of_type(body, IInteractor) as IInteractor
	#if interactor:
		#iInteractable.set_interactor(interactor)
		#print(body.name, " entered trigger!")
#
#func _on_area_2d_body_exited(body: Node2D) -> void:
	## if same interactor leaves
	#var interactor = Utils.try_get_child_of_type(body, IInteractor) as IInteractor
	#if interactor:
		#iInteractable.clear_interactor()
		#print(body.name, " exited trigger!")
		#

	
#func complete(iInteractor: IInteractor):
	#destroy(self)
	#
#func destroy(caller: Node2D):
	
