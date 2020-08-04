extends Button


func _on_Join_button_up() -> void:
	var ret = get_node("/root/Multiplayer_node").JoinGame(get_node("../Address").text)
	disabled = ret
	get_node("../Leave").disabled = not ret
	get_node("../Host").disabled = ret
