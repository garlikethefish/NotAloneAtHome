extends Node2D

@export var player: Node2D
@export var max_active_lights: int = 4
@export var max_distance: float = 200.0
@export var fade: bool = true
@export var update_interval: float = 0.1

var lights: Array[Light2D] = []
var timer: float = 0.0
@onready var max_dist_sq: float = max_distance * max_distance

func _ready() -> void:
	# Initial fetch of lights
	for node in get_tree().get_nodes_in_group("dynamic_lights"):
		if node is Light2D:
			_setup_light(node)

	if not player:
		push_warning("LightManager2D: No player assigned. Script will idle.")

func _process(delta: float) -> void:
	if not player: return

	timer += delta
	if timer < update_interval:
		return
	timer = 0.0

	_update_lights()

func _update_lights() -> void:
	var player_pos = player.global_position
	
	# 1. Sort lights by squared distance (faster)
	lights.sort_custom(func(a, b): 
		return a.global_position.distance_squared_to(player_pos) < b.global_position.distance_squared_to(player_pos)
	)

	# 2. Iterate and apply logic
	for i in range(lights.size()):
		var light = lights[i]
		var dist_sq = light.global_position.distance_squared_to(player_pos)
		var original_energy = light.get_meta("original_energy", 1.0)

		# Check if it qualifies for being ON
		if i < max_active_lights and dist_sq <= max_dist_sq:
			light.enabled = true
			if fade:
				var dist = sqrt(dist_sq) # Only calculate SQRT for the few active lights
				var t = clamp(1.0 - dist / max_distance, 0.0, 1.0)
				light.energy = t * original_energy
			else:
				light.energy = original_energy
		else:
			# Explicitly turn off and reset if it's too far or too many are active
			light.enabled = false
			light.energy = 0.0

func _setup_light(light: Light2D) -> void:
	if not lights.has(light):
		lights.append(light)
		if not light.has_meta("original_energy"):
			light.set_meta("original_energy", light.energy)

func register_light(light: Light2D) -> void:
	_setup_light(light)
