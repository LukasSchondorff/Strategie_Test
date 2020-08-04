extends Button

var Main_class : Script
var Main_node : Node

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	Main_class = preload("res://src/Multiplayer/Main.cs")
	Main_node = Main_class.new()
	add_child(Main_node)


func _on_Host_button_up() -> void:
	Main_node.HostGame(get_node("../Name").text)
