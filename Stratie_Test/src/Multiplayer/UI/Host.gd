extends Button


func _on_Host_button_up() -> void:
	get_node("/root/Multiplayer_node").HostGame(get_node("../Name").text)

