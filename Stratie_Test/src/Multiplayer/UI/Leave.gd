extends Button

func _ready() -> void:
	disabled = true


func _on_Leave_button_up() -> void:
	var ret = get_node("/root/Multiplayer_node").LeaveGame()
	disabled = ret
	get_node("../Host").disabled = not ret
	get_node("../Join").disabled = not ret
