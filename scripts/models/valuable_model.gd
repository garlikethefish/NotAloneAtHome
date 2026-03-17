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
	Cabinet,
	Sink,
	Fridge,
	Rug1,
	Rug2,
	Pillow,
	Small_Lamp,
	Tall_Lamp1,
	Tall_Lamp2,
	Bowl,
	Broom,
	Kitchen_Rack,
	None
}

var sprite: Sprite2D
var value: int

func _init(texture: Texture2D, itemValue: int):
	var tempSprite = Sprite2D.new()
	tempSprite.texture = texture
	self.sprite = tempSprite
	self.value = itemValue
