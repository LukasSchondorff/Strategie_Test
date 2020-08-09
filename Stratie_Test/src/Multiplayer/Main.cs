using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
	private readonly int default_port = 7777;
	private readonly int max_players = 4;

	public string PlayerName { get; set; }

	private string ServerAddress { get; set; }

	private Dictionary<int, string> Players = new Dictionary<int, string>();

	[Signal]
	public delegate void ErrorSignal(string message);
	[Signal]
	public delegate void SuccessSignal(string message);
	[Signal]
	public delegate void InfoSignal(string message);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("connection_failed", this, nameof(ConnectionFailed));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected), null, (int)Godot.Object.ConnectFlags.Deferred);
	}

	public bool HostGame(string name) 
	{
		if (name.Empty()){
			EmitSignal(nameof(ErrorSignal), "Please enter a name!");
			return false;
		}
		PlayerName = name;

		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(default_port, max_players);
		GetTree().NetworkPeer = peer;

		EmitSignal(nameof(SuccessSignal), "You are now hosting.");

		return true;
	}

	public bool JoinGame(string address, string name) 
	{
		if (address.Empty() || name.Empty())
		{
			EmitSignal(nameof(ErrorSignal), "Please enter a name and an address!");
			return false;
		}
		GD.Print($"Joining game with address {address}");

		PlayerName = name;
		ServerAddress = address;

		var clientPeer = new NetworkedMultiplayerENet();
		var result = clientPeer.CreateClient(address, default_port);

		if(result != 0) 
		{
			EmitSignal(nameof(ErrorSignal), $"Connection failed! ({result.ToString()})");
			return false;
		}

		GetTree().NetworkPeer = clientPeer;
		EmitSignal(nameof(SuccessSignal), "Connecting...");

		return true;
	}

	public bool LeaveGame() 
	{
		if(GetTree().NetworkPeer == null) 
		{
			EmitSignal(nameof(ErrorSignal), "No current connection!");
			return false;
		}

		Players.Clear();

		Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());

		((NetworkedMultiplayerENet) GetTree().NetworkPeer).CloseConnection();
		GetTree().NetworkPeer = null;

		EmitSignal(nameof(SuccessSignal), "Disconnected!");

		return true;
	}

	public void PlayerConnected(int id)
	{
		GD.Print($"tell other player my name is {PlayerName}");

		RpcId(id, nameof(RegisterPlayer), PlayerName);
	}

	public void PlayerDisconnected(int id) 
	{
		GD.Print($"Player {id} disconnected");

		RemovePlayer(id);
	}

	public void ConnectedToServer() 
	{
		EmitSignal(nameof(SuccessSignal), "Successfully connected to server");
	}

	public void ConnectionFailed() 
	{
		GetTree().NetworkPeer = null;

		EmitSignal(nameof(ErrorSignal), "Failed to connect");
	}

	public void ServerDisconnected() 
	{
		Players.Clear();
		GetTree().NetworkPeer = null;

		var clientPeer = new NetworkedMultiplayerENet();
		Error result = 0;
		for (int i = 0; i < 10; i++){
			result = clientPeer.CreateClient(ServerAddress, default_port);

			if(result != 0) 
			{
				EmitSignal(nameof(ErrorSignal), $"Connection failed! ({result.ToString()})");
				System.Threading.Thread.Sleep(1000);
			}
			else 
			{
				break;
			}
		}

		if (result == 0){
			GetTree().NetworkPeer = clientPeer;
			EmitSignal(nameof(SuccessSignal), "Connecting...");
			return;
		}

		EmitSignal(nameof(ErrorSignal), "Disconnected from the server");
	}

	public void ConnectionClosed() 
	{
		EmitSignal(nameof(ErrorSignal), "Connection closed");
	}

	[Remote]
	private void RegisterPlayer(string playerName) 
	{
		var id = GetTree().GetRpcSenderId();

		Players.Add(id, playerName);

		GD.Print($"{playerName} added with Id {id}");
	}

	[Remote]
	private void RemovePlayer(int id)
	{
		if (Players.ContainsKey(id))
		{
			string name;
			Players.TryGetValue(id, out name);
			EmitSignal(nameof(InfoSignal), $"{name} left the game");
			Players.Remove(id);
		} 
		else 
		{
			GD.Print($"Player {id} not found");
		}
	}

}
