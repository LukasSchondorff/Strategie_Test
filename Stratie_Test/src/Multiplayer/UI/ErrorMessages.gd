extends RichTextLabel

func _ready() -> void:
	if get_node("/root/Multiplayer_node").connect("ErrorSignal", self, "_on_Error") != OK:
		return
	if get_node("/root/Multiplayer_node").connect("SuccessSignal", self, "_on_Success") != OK:
		return

func _on_Error(message: String) -> void:
	bbcode_text = "[color=red]" + message + "[/color]"
	lobby_status_changed(false)

func _on_Success(message: String) -> void:
	bbcode_text = "[color=green]" + message + "[/color]"
	if message == "Successfully connected to server":
		lobby_status_changed(true)
		get_node("../../MultiplayerChat/Lower/MessageText").grab_focus()
	if message == "Disconnected!":
		lobby_status_changed(false)

func lobby_status_changed(inLobby: bool) -> void:
	for n in get_tree().get_nodes_in_group("InLobby"):
		if n is Button:
			n.disabled = not inLobby;
		else:
			n.editable = inLobby;
	for n in get_tree().get_nodes_in_group("BeforeLobby"):
		if n is Button:
			n.disabled = inLobby;
		else:
			n.editable = not inLobby;


