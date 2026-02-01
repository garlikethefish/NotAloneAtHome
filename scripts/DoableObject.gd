extends Node2D
class_name InteractableObject

@export var valuable: Valuable.ValuableType = Valuable.ValuableType.None
var isValuable: bool:
	get: return valuable != Valuable.ValuableType.None

@export var interactionSprite: Sprite2D
@export var objectSprite: Sprite2D
@export var interactionSpriteTexture: Texture2D
@export var objectSpriteTexture: Texture2D

@export var tapsTillDone: int = 10
@export var isCarriable = false

@export var isKitty = false
@export var isDeadThief = false
@export var isThiefsCloset = false
@export var isPc = false

var isCarried = false
var isCompleteTriggered: bool = false
var tween
var allowInteraction: bool
var isKillingItself: bool
@onready var interactionKeyStartPos: Vector2 = interactionSprite.position
@onready var interactionKeyStartScale: Vector2 = interactionSprite.scale
var interactionKeyEndPos:
	get:
		return interactionKeyStartPos - Vector2(0, -20)
var player: PlayerCharacter

signal onComplete()

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if isKitty and isThiefsCloset:
		isCarriable = false
		
	if isDeadThief:
		isCarriable = true
		
	# set sprites
	interactionSprite.texture = interactionSpriteTexture
	objectSprite.texture = objectSpriteTexture
	interactionSprite.visible = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	allowInteraction = (
		isThiefsCloset and player and player.carriableObject and player.carriableObject.isDeadThief
		or
		player and !player.isCarringObject and !player.mask_on and !isKillingItself
	)
	interactionSprite.visible = allowInteraction
	
	if isKillingItself: return
	var isInteractPressed = Input.is_action_just_pressed("interact")
	
	if isPc and isInteractPressed and allowInteraction:
		GameManager.laptop.show_overlay()
	
	# destroy dead thief in closet
	if (
		isThiefsCloset and 
		player and 
		player.carriableObject and 
		player.carriableObject.isDeadThief and 
		isInteractPressed
	):
		isCarried = false
		#global_position = player.global_position #+ player.direction
		interactionSprite.visible = true

		player.carriableObject.destroy(self)
		player.isCarringObject = false
		player.carriableObject = null
		GameManager.onThiefHidden.emit()
		return
		
	if isCarried:
		stickToPlayer()
		
		# drop item
		if player and (isInteractPressed or player.mask_on) and !isDeadThief:
			isInteractPressed = false
			dropItem()
	
	if !isCarried and player and isInteractPressed:
		tweenAnimation()
		
		# pick up
		if isCarriable and allowInteraction:
			player.isCarringObject = true
			player.carriableObject = self
			interactionSprite.visible = false
			isCarried = true
		
		# 
		if !isCarriable and !isThiefsCloset and !isPc:
			tapsTillDone -= 1
		#print("Interact key pressed!")
		
	if tapsTillDone <= 0 and !isCompleteTriggered:
		complete()

func dropItem():
	isCarried = false
	global_position = player.global_position #+ player.direction
	interactionSprite.visible = true
	player.isCarringObject = false
	player.carriableObject = null
	

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
	
	
func complete():
	isCompleteTriggered = true
	GameManager.collectTrash()
	emit_signal("onComplete")
	if isKitty:
		GameManager.onKittyFed.emit()
		
	destroy(self)
	
func destroy(caller: Node2D):
	isKillingItself = true
	
	if !tween:
		tween = create_tween()
	else:
		tween.kill()
		tween = create_tween()
	
	var endPos = caller.global_position
	tween.set_parallel(true)
	tween.tween_property(objectSprite, "scale", Vector2(0,0), .5)
	tween.tween_property(objectSprite, "rotation", 360, 1)
	tween.tween_property(self, "global_position", endPos, .5)
	
	tween.finished.connect(func():
		print("Tween finished!")
		queue_free() # Destroys itself 
	)
	
func stickToPlayer():
	if player:
		global_position = player.global_position
