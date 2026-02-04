extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


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
