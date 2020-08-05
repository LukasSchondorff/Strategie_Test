using Godot;
using System;

public class Chat : Node
{
	private RichTextLabel ChatArea;
	private Button SendButton;
	private LineEdit MessageText;

	public override void _Ready()
	{
		ChatArea = (RichTextLabel) GetNode("ChatArea");
		MessageText = (LineEdit) GetNode("Lower/MessageText");
		SendButton = (Button) GetNode("Lower/Send");

		SendButton.Connect("button_up", this, nameof(SendMessage));
	}

	[Remote]
	private void ReceiveMessage(string message, string name)
	{
		// public variable, didn't find a better solution
		ChatArea.AddText(name + ": " + message);
		ChatArea.Newline();
	}

	private void SendMessage()
	{
		Rpc(nameof(ReceiveMessage), MessageText.Text, GetNode("/root/Multiplayer_node").Get("PlayerName"));
		ChatArea.AddText(GetNode("/root/Multiplayer_node").Get("PlayerName") + ": " + MessageText.Text);
		MessageText.Clear();
		ChatArea.Newline();
	}
}
