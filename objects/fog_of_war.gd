extends Node2D

@export var player_vision_node: Node2D 
@export var player_camera_node: Camera2D 

@onready var player_mirror_vision_node = $SubViewportContainer/SubViewport/PlayerVisionMirror
@onready var player_following_camera = $SubViewportContainer/SubViewport/FollowingCamera

func _ready():
	player_mirror_vision_node.source      = player_vision_node
	player_following_camera.follow_camera = player_camera_node

func _process(_delta):
	global_position = player_camera_node.global_position
