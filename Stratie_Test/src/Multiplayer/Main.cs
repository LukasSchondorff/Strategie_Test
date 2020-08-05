using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
    private readonly int default_port = 7777;
    private readonly int max_players = 4;

    public string PlayerName { get; set; }

    private Dictionary<int, string> Players = new Dictionary<int, string>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("connection_failed", this, nameof(ConnectionFailed));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    public bool HostGame(string name) 
    {
        if (name.Empty()){
            GD.Print("Please enter a name!");
            return false;
        }
        PlayerName = name;

        var peer = new NetworkedMultiplayerENet();
        peer.CreateServer(default_port, max_players);
        GetTree().NetworkPeer = peer;

        GD.Print("You are now hosting.");

        return true;
    }

    public bool JoinGame(string address, string name) 
    {
        if (address.Empty())
        {
            GD.Print("Please enter an address!");
            return false;
        }
        GD.Print($"Joining game with address {address}");

        PlayerName = name;

        var clientPeer = new NetworkedMultiplayerENet();
        var result = clientPeer.CreateClient(address, default_port);

        if(result != 0) 
        {
            GD.Print("Failure!");
        }

        GetTree().NetworkPeer = clientPeer;

        return true;
    }

    public bool LeaveGame() 
    {
        if(GetTree().NetworkPeer == null) 
        {
            return false;
        }
        GD.Print("Leaving current game");

        Players.Clear();

        Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());

        ((NetworkedMultiplayerENet) GetTree().NetworkPeer).CloseConnection();
        GetTree().NetworkPeer = null;

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
        GD.Print("Successfully connected to the server");
    }

    public void ConnectionFailed() 
    {
        GetTree().NetworkPeer = null;

        GD.Print("Failed to connect");
    }

    public void ServerDisconnected() 
    {
        GD.Print("Disconnected from the server");
    }

    public void ConnectionClosed() 
    {
        GD.Print("Disconnected from the server");
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
            Players.Remove(id);
        } 
        else 
        {
            GD.Print($"Player {id} not found");
        }
    }

}