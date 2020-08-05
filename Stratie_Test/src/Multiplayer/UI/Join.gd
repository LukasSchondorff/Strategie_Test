extends Button


func _on_Join_button_up() -> void:
	var ret = get_node("/root/Multiplayer_node").JoinGame(get_node("../Address").text, get_node("../Name").text)
	disabled = ret
	get_node("../Leave").disabled = not ret
	get_node("../Host").disabled = ret
	get_node("../Name").editable = not ret
	get_node("../Address").editable = not ret
	get_node("../../MultiplayerChat/Lower/Send").disabled = not ret
