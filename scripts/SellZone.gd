extends Node2D

var valuable: InteractableObject
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass
	
func _on_area_2d_area_entered(area: Area2D) -> void:
	var parent = area.get_parent()
	if parent is InteractableObject:
		
		if !parent.isValuable: return
		
		# Sell carried object
		var itemCost = GameManager.valuables[parent.valuable].value
		GameManager.stolen_stuff_amount += 1
		GameManager.money_lost -= itemCost
		
		# destroy object
		parent.dropItem()
		parent.destroy(self)
		GameManager.onItemStealed.emit()
