extends Node
class_name InteractableObject

@export var interactionSprite: Sprite2D
@export var objectSprite: Sprite2D
@export var tapsTillDone: int = 10
var isCompleteTriggered: bool = false
var tween
@onready var interactionKeyStartPos: Vector2 = interactionSprite.position
@onready var interactionKeyStartScale: Vector2 = interactionSprite.scale
var interactionKeyEndPos:
	get:
		return interactionKeyStartPos / 2
var player: PlayerCharacter

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	interactionSprite.visible = false
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if player and Input.is_action_just_pressed("interact"):
		tweenAnimation()
		tapsTillDone -= 1
		print("Interact key pressed!")
		
	if tapsTillDone <= 0 and !isCompleteTriggered:
		onComplete()


func _on_area_2d_body_entered(body: Node2D) -> void:
	if body is PlayerCharacter:
		player = body
		interactionSprite.visible = true
		print(body.name, "entered trigger!")

func _on_area_2d_body_exited(body: Node2D) -> void:
	if body is PlayerCharacter:
		player = null
		interactionSprite.visible = false
		print(body.name, "exited trigger!")
		
		
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
	tween.tween_property(interactionSprite, "position", interactionKeyEndPos, .1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale / 2, .1)

	tween.tween_property(interactionSprite, "position", interactionKeyStartPos, .1).set_delay(.1)
	tween.tween_property(interactionSprite, "scale", interactionKeyStartScale, .1).set_delay(.1)
	
	
func onComplete():
	isCompleteTriggered = true
	print('task done!')
	queue_free() # Destroys itself 
