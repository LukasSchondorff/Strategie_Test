extends Button

export(String, FILE, "*.tscn") var map_scene_path = "res://Map/Spatial.tscn"
var map_scene : PackedScene = load(map_scene_path)

func _ready() -> void:
	disabled = true

func _on_GenerateMap_button_up() -> void:
	if get_tree().change_scene_to(map_scene) != OK:
		print("something went wrong")
