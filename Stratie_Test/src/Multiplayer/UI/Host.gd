extends Button


func _on_Host_button_up() -> void:
	var ret = get_node("/root/Multiplayer_node").HostGame(get_node("../Name").text)
	disabled = ret
	get_node("../Leave").disabled = not ret
	get_node("../Join").disabled = ret
	get_node("../../MultiplayerChat/Lower/Send").disabled = not ret
