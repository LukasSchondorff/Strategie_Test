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
	private void ReceiveMessage(string message)
	{	
		AddMessage(message);
	}

	private void SendMessage()
	{
		string message = $"[color=yellow]{GetNode("/root/Multiplayer_node").Get("PlayerName"),-10}:[/color] {MessageText.Text}";
		Rpc(nameof(ReceiveMessage), message);
		AddMessage(message);
		MessageText.Clear();
		ChatArea.Newline();
	}

	private void AddMessage(string message)
	{
		ChatArea.AppendBbcode(message);
		ChatArea.Newline();
	}
}
