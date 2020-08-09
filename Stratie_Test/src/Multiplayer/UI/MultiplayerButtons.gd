extends VBoxContainer

func _ready() -> void:
	get_node("/root/Multiplayer_node").connect("ReadyToSendAttributes", self, "enableJoinMap")

func enableJoinMap() -> void:
	get_node("GenerateMap").visible = true
	get_node("GenerateMap").text = "Join Map"
