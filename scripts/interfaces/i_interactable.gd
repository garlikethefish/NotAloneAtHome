extends Helper

class_name IInteractible

signal on_interaction(_interactor: IInteractor)
signal on_interactor_clear(_interactor: IInteractor)
signal on_interactor_set(_interactor: IInteractor)

## @param {IInteractor}
var can_be_interacted_with_callable: Callable

@onready var interactionArea: Area2D = $Area2D
@onready var interactionSprite: Sprite2D = $InteractionKey
@onready var interactionKeyStartPos: Vector2 = interactionSprite.position
@onready var interactionKeyStartScale: Vector2 = interactionSprite.scale

var interaction_sprite_tween: Tween
var interactor: IInteractor

func _ready():
	interactionSprite.visible = false
	interactionArea.scale = Vector2.ONE * 2
	
func interact(_interactor: IInteractor):
	if !can_be_interacted_with(): return
	print("Interacted with " + helper_holder.main_parent.name + " " + helper_holder.main_parent.get_script().get_global_name())
	tweenAnimation()
	on_interaction.emit(_interactor)
	
func set_interactor(_interactor: IInteractor):
	interactor = _interactor
	on_interactor_set.emit(interactor)
	interactionSprite.visible = can_be_interacted_with()
	#_iInteractor.on_interactor_status_update.connect(update_can_interact_status)
	#print("Set InteracTOR")
	
func clear_interactor():
	if !interactor: return
	interactionSprite.visible = false
	on_interactor_clear.emit(interactor)
	#interactor.on_interactor_status_update.disconnect(update_can_interact_status)
	interactor = null
	#print("Cleared InteracTOR")

func update_can_interact_status():
	interactionSprite.visible = can_be_interacted_with()
	
func tweenAnimation(): 
	# reset pos
	interactionSprite.position = interactionKeyStartPos
	interactionSprite.scale = interactionKeyStartScale
	
	if !interaction_sprite_tween:
		interaction_sprite_tween = create_tween()
	else:
		interaction_sprite_tween.kill()
		interaction_sprite_tween = create_tween()
	
	interaction_sprite_tween.set_parallel(true)
	interaction_sprite_tween.tween_property(interactionSprite, "position", interactionKeyStartPos - Vector2(0, -20), .1)
	interaction_sprite_tween.tween_property(interactionSprite, "scale", interactionKeyStartScale / 2, .1)

	interaction_sprite_tween.tween_property(interactionSprite, "position", interactionKeyStartPos, .1).set_delay(.1)
	interaction_sprite_tween.tween_property(interactionSprite, "scale", interactionKeyStartScale, .1).set_delay(.1)
	
func can_be_interacted_with(_interactor: IInteractor = interactor) -> bool:
	return can_be_interacted_with_callable.call(_interactor)
	
func assert_assigned_callables():
	assert_callable(can_be_interacted_with_callable, "can_be_interacted_with_callable")
