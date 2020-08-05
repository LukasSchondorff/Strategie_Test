extends RichTextLabel

func _ready() -> void:
	get_node("/root/Multiplayer_node").connect("ErrorSignal", self, "_on_Error")
	get_node("/root/Multiplayer_node").connect("SuccessSignal", self, "_on_Success")

func _on_Error(message: String) -> void:
	bbcode_text = "[color=red]" + message + "[/color]"

func _on_Success(message: String) -> void:
	bbcode_text = "[color=green]" + message + "[/color]"
