using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
    private readonly int default_port = 7777;
    private readonly int max_players = 4;

    private string PlayerName { get; set; }

    private Dictionary<int, string> Players = new Dictionary<int, string>();

    private Button HostButton { get; set; }
	private Button JoinButton { get; set; }
	private Button LeaveButton { get; set; }
	private TextEdit NameText { get; set; }
	private TextEdit AddressText { get; set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("connection_failed", this, nameof(ConnectionFailed));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    public void HostGame(string name) {
        if (name.Empty()){
            GD.Print("Please enter a name!");
            return;
        }
        PlayerName = name;

        var peer = new NetworkedMultiplayerENet();
        peer.CreateServer(default_port, max_players);
        GetTree().NetworkPeer = peer;

        GD.Print("You are now hosting.");
    }

    public void JoinGame(string address) {
        if (address.Empty()){
            GD.Print("Please enter an address!");
            return;
        }
        GD.Print($"Joining game with address {address}");

        var clientPeer = new NetworkedMultiplayerENet();
        var result = clientPeer.CreateClient(address, default_port);

        GetTree().NetworkPeer = clientPeer;
    }

    public void LeaveGame() {
        GD.Print("Leaving current game");

        // "Delete" all player nodes
        foreach(var player in Players){
            GetNode(player.Key.ToString()).QueueFree();
        }

        Players.Clear();

        // "Delete" network node
        GetNode(GetTree().GetNetworkUniqueId().ToString()).QueueFree();

        Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());

        ((NetworkedMultiplayerENet) GetTree().NetworkPeer).CloseConnection();
        GetTree().NetworkPeer = null;
    }

    public void PlayerConnected() {

    }

    public void PlayerDisconnected() {

    }

    public void ConnectedToServer() {

    }

    public void ConnectionFailed() {

    }

    public void ServerDisconnected() {

    }

    [Remote]
    private void RegisterPlayer(String playerName) {

    }

    [Remote]
    private void RemovePlayer(int id){

    }

}
