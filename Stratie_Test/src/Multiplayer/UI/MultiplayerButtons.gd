extends VBoxContainer

func _ready() -> void:
	get_node("/root/MapSync").connect("MayJoin", self, "enableJoinMap")

func enableJoinMap() -> void:
	get_node("GenerateMap").visible = true
	get_node("GenerateMap").text = "Join Map"
