extends RichTextLabel

func _ready() -> void:
	get_node("/root/Multiplayer_node").connect("ErrorSignal", self, "_on_Error")
	get_node("/root/Multiplayer_node").connect("SuccessSignal", self, "_on_Success")

func _on_Error(message: String) -> void:
	bbcode_text = "[color=red]" + message + "[/color]"
	get_node("../Host").disabled = false
	get_node("../Join").disabled = false
	get_node("../Leave").disabled = true
	get_node("../Name").editable = true
	get_node("../Address").editable = true
	get_node("../../MultiplayerChat/Lower/Send").disabled = true

func _on_Success(message: String) -> void:
	bbcode_text = "[color=green]" + message + "[/color]"
	if message == "Successfully connected to server":
		get_node("../Host").disabled = true
		get_node("../Leave").disabled = false
		get_node("../Join").disabled = true
		get_node("../Name").editable = false
		get_node("../Address").editable = false
		get_node("../../MultiplayerChat/Lower/Send").disabled = false
