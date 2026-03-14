extends Helper

class_name IInteractible

signal on_interaction(_interactor: IInteractor)

var can_be_interacted_with_callable: Callable

@onready var interactionSprite: Sprite2D = $InteractionKey
@onready var interactionKeyStartPos: Vector2 = interactionSprite.position
@onready var interactionKeyStartScale: Vector2 = interactionSprite.scale

var apear_tween: Tween

func _ready():
	interactionSprite.visible = false
	sprite_hide_animation()


func show_sprite():
	sprite_apear_animation()


func hide_sprite():
	sprite_hide_animation()

func interact(_interactor: IInteractor):
	if (
		!can_be_interacted_with(_interactor)
	): return
	
	tweenAnimation()
	on_interaction.emit(_interactor)


func tweenAnimation(): 
	interactionSprite.position = interactionKeyStartPos
	interactionSprite.scale = interactionKeyStartScale
	
	var tween = create_tween()
	
	tween.set_parallel(true)
	tween.tween_property(interactionSprite, "position", interactionKeyStartPos - Vector2(0, -20), .1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale / 2, .1)

	tween.tween_property(interactionSprite, "position", interactionKeyStartPos, .1).set_delay(.1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale, .1).set_delay(.1)


func sprite_apear_animation():
	interactionSprite.visible = true
	if apear_tween: apear_tween.kill()
	apear_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_ELASTIC)
	apear_tween.tween_property(interactionSprite, "scale", Vector2.ONE, .5)
	


func sprite_hide_animation():
	if apear_tween: apear_tween.kill()
	apear_tween = create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_ELASTIC)
	apear_tween.tween_property(interactionSprite, "scale", Vector2.ZERO, .2)
	await apear_tween.finished
	interactionSprite.visible = false


func can_be_interacted_with(_interactor: IInteractor) -> bool:
	return can_be_interacted_with_callable.call(_interactor)


func assert_assigned_callables():
	assert_callable(can_be_interacted_with_callable, "can_be_interacted_with_callable")
