extends Button


func _on_Join_button_up() -> void:
	get_node("/root/Multiplayer_node").JoinGame(get_node("../Address").text, get_node("../Name").text)
