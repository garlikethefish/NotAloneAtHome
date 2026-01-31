extends Node
class_name Valuable

enum ValuableType {
	Table,
	Chair,
	TV,
	Sofa,
	Dresser,
	Closet,
	None
}

var sprite: Sprite2D
var value: int

func _init(texture: Texture2D, value: int):
	var tempSprite = Sprite2D.new()
	tempSprite.texture = texture
	self.sprite = tempSprite
	self.value = value
