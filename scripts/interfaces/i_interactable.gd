extends Node2D
class_name IInteractible

signal on_interaction(iInteractor: IInteractor)
signal on_interactor_clear(iInteractor: IInteractor)
signal on_interactor_set(iInteractor: IInteractor)

@onready var interactionArea: Area2D = $Area2D
@onready var interactionSprite: Sprite2D = $InteractionKey
@onready var interactionKeyStartPos: Vector2 = interactionSprite.position
@onready var interactionKeyStartScale: Vector2 = interactionSprite.scale

var tween
var iInteractor: IInteractor

func _ready():
	interactionSprite.visible = false
	interactionArea.scale = Vector2.ONE * 2

func interact(_iInteractor: IInteractor):
	if !can_interact(_iInteractor): return
	tweenAnimation()
	on_interaction.emit(iInteractor)
	
func set_interactor(_iInteractor: IInteractor):
	iInteractor = _iInteractor
	on_interactor_set.emit(iInteractor)
	interactionSprite.visible = can_interact(_iInteractor)
	_iInteractor.on_interactor_status_update.connect(update_can_interact_status)
	print("Set InteracTOR")
	
func clear_interactor():
	if !iInteractor: return
	interactionSprite.visible = false
	on_interactor_clear.emit(iInteractor)
	iInteractor.on_interactor_status_update.disconnect(update_can_interact_status)
	iInteractor = null
	print("Cleared InteracTOR")
	
func can_interact(_interactor: IInteractor) -> bool:
	var parent := get_parent()
	return parent != null and parent.has_method("can_interact") and parent.can_interact(_interactor) and _interactor.can_interact(self)
	
func update_can_interact_status():
	interactionSprite.visible = can_interact(iInteractor)
	
func tweenAnimation(): 
	# reset pos
	interactionSprite.position = interactionKeyStartPos
	interactionSprite.scale = interactionKeyStartScale
	
	if !tween:
		tween = create_tween()
	else:
		tween.kill()
		tween = create_tween()
	
	tween.set_parallel(true)
	tween.tween_property(interactionSprite, "position", interactionKeyStartPos - Vector2(0, -20), .1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale / 2, .1)

	tween.tween_property(interactionSprite, "position", interactionKeyStartPos, .1).set_delay(.1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale, .1).set_delay(.1)
	
