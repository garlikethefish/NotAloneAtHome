extends TextureRect

@onready var subViewport: SubViewport = $"../SubViewport"
func _ready():
	texture = subViewport.get_texture()
	size = subViewport.size
