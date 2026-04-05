extends SubViewportContainer
@export var player_vision_node:    Node2D 
@export var player_camera_node:    Camera2D 
@export var visible_area_polygons: Array[Polygon2D]

@onready var player_mirror_vision_node   = $SubViewport/PlayerVisionMirror
@onready var player_following_camera     = $SubViewport/FollowingCamera
@onready var visible_area_polygon_drawer = $SubViewport/VisibleAreaPolygonDrawer

func _ready():
	player_mirror_vision_node.source      = player_vision_node
	player_following_camera.follow_camera = player_camera_node
	visible_area_polygon_drawer.polygons  = visible_area_polygons
