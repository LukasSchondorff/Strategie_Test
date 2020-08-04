extends VBoxContainer

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var Main_class = preload("res://src/Multiplayer/Main.cs")
	var Main_node = Main_class.new()
	Main_node.name = "Multiplayer_node"
	get_tree().get_root().call_deferred("add_child", Main_node)
