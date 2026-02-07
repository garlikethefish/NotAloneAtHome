extends Node2D

class_name SellZone

var hasApeared = true
var tween: Tween
var startingScale: Vector2
var startingPos: Vector2
@onready var sprite: Sprite2D = $Sprite2D

@export var expandToPos: Vector2 = Vector2(0,0)
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	print("start this")
	startingScale = scale
	startingPos = global_position
	disapear()
	GameManager.sellZone = self


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass
	
func _on_area_2d_area_entered(area: Area2D) -> void:
	var valuable: ValuableObject = Utils.find_parent_of_type(area, ValuableObject)
	if !valuable: return
	
	# Sell carried object
	var itemCost = GameManager.valuables[valuable.type].value
	GameManager.stolen_stuff_amount += 1
	GameManager.money_lost -= itemCost
	
	# destroy object
	valuable.sell(self)
	
	GameManager.on_item_steal.emit()
	GameManager.suspicion = clamp(GameManager.suspicion - 10, 0, 100)
	
	if GameManager.stolen_stuff_amount >= GameManager.maxStealableItems:
		GameManager.on_max_items_stolen.emit()

func apear():
	if hasApeared: return
	print("Apear")
	hasApeared = true
	if tween:
		tween.kill()
		
	tween = create_tween()
	tween.set_parallel()
	tween.tween_property(self, "scale", startingScale, .5).set_trans(Tween.TRANS_CUBIC) # fade out
	tween.tween_property(self, "global_position", startingPos + expandToPos, .5).set_trans(Tween.TRANS_CUBIC) # fade out

	
func disapear():
	if !hasApeared: return
	print("no apear")
	hasApeared = false
	if tween:
		tween.kill()
		
	tween = create_tween()
	tween.set_parallel()
	tween.tween_property(self, "scale", Vector2(0, 0), .5).set_trans(Tween.TRANS_CUBIC) # fade out
	tween.tween_property(self, "global_position", startingPos, .5).set_trans(Tween.TRANS_CUBIC) # fade out

	
