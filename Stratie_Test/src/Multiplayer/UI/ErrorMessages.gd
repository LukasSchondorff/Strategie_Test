extends RichTextLabel

func _ready() -> void:
	get_node("/root/Multiplayer_node").connect("ErrorSignal", self, "_on_Error")

func _on_Error(message: String) -> void:
	bbcode_text = "[color=red]" + message + "[/color]"
