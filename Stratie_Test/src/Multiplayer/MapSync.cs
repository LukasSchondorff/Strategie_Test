using Godot;
using System;

public class MapSync : Node
{
    [Signal]
    public delegate void ReadyToSendAttributes(); 

    [Signal]
    public delegate void MayJoin(); 
    public bool AbleToJoin = false;

    public struct MapAttributes{
        public MapAttributes(int seed, int octaves, float period, float lacunarity, float persistence, int width, float height, Vector3 CellSize, float tree_spread){
            this.seed = seed;
            this.octaves = octaves;
            this.period = period;
            this.lacunarity = lacunarity;
            this.persistence = persistence;
            this.width = width;
            this.height = height;
            this.CellSize = CellSize;
            this.tree_spread = tree_spread;
        }
        int seed;
        int octaves;
        float period;
        float lacunarity;
        float persistence;
        int width;
        float height;
        Vector3 CellSize;
        float tree_spread;
    }

    private MapAttributes attributes;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        attributes = new MapAttributes();
        Connect(nameof(ReadyToSendAttributes), this, nameof(JoiningEnabled));
    }

    public void SetAttributes(int seed, int octaves, float period, float lacunarity, float persistence, int width, float height, Vector3 CellSize, float tree_spread){
        attributes = new MapAttributes(seed, octaves, period, lacunarity, persistence, width, height, CellSize, tree_spread);
    }

    public void JoiningEnabled(){
        AbleToJoin = true;
        Rpc(nameof(EnableJoining));
    }

    [Remote]
    public void EnableJoining(){
        EmitSignal(nameof(MayJoin));
    }
}
