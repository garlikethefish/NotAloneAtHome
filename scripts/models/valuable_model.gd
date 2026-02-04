extends Node
class_name ValuableModel

enum Valuable {
	TV,
	Bed,
	Chair1,
	Chair2,
	Closet,
	Sofa,
	Table,
	Vase,
	None
}

var sprite: Sprite2D
var value: int

func _init(texture: Texture2D, itemValue: int):
	var tempSprite = Sprite2D.new()
	tempSprite.texture = texture
	self.sprite = tempSprite
	self.value = itemValue
