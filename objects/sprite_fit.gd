extends Sprite2D  # This is the correct class for sprites in Godot 4.6

@export var area2d: Area2D  # Reference to your Area2D node
@export var collision_shape: CollisionShape2D  # Reference to the CollisionShape2D in the Area2D

func _ready():
	# Call the function to fit the sprite to the area when the scene starts
	_fit_sprite_to_area()

func _process(delta):
	# If the area size changes dynamically (e.g., resizing), update the scale
	_fit_sprite_to_area()

# Function to scale the sprite based on Area2D's CollisionShape2D size
func _fit_sprite_to_area():
	# Get the shape of the CollisionShape2D
	var shape = collision_shape.shape
	
	# Variable to store the area size
	var area_size = Vector2.ZERO
	
	if shape is RectangleShape2D:
		# If the shape is a rectangle, get its extents (half width, half height)
		area_size = shape.extents * 2  # Full width and height
	
	elif shape is CircleShape2D:
		# If the shape is a circle, get the radius
		var radius = shape.radius
		area_size = Vector2(radius * 2, radius * 2)  # Full width and height of the bounding box
	
	# Get the sprite's original size (texture size)
	var sprite_size = texture.get_size()
	
	# Calculate the scale factor to fit the sprite inside the area
	var scale_factor = min(area_size.x / sprite_size.x, area_size.y / sprite_size.y)
	
	# Apply the scale to the sprite
	scale = Vector2(scale_factor, scale_factor)
