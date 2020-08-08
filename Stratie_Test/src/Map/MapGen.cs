using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class MapGen : GridMap
{
	[Export(PropertyHint.Range, "1,15,or_greater")]
	private int chunk_number = 5;
	
	[Export(PropertyHint.Range, "-1,1,0.01")]
	private float Hill_Fatness = 0f;
	
	[Export(PropertyHint.Range, "1,100,or_greater")]
	private int Hill_Tallness = 100;

	[Export(PropertyHint.Range, "-1,1,0.01")]
	private float tree_spread = -0.5f;
	int chunk_loader = 32;
	private Vector3 cell_size = new Vector3(3f,1.5f,3f);
	int width;
	float height;
	int length;
	OpenSimplexNoise open_simplex_new;
	float colision_area_factor = 1.5f;
	Vector3 pos1;
	Vector3 pos2;
	System.Threading.Mutex mutex;
	
	private PlayerLevel playerlevel;

	[Signal]
	public delegate void ReadySignal();

	public override void _Ready()
	{
		width = chunk_number*chunk_loader*(int)CellSize.x;
		length = width;
		height = 1f;
		open_simplex_new = new OpenSimplexNoise();
		CellSize = cell_size;

		if (GetTree().NetworkPeer != null && GetTree().NetworkPeer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected && !IsNetworkMaster()){
			RpcId(1, nameof(GetAttributes));
			
			var peer = GetTree().NetworkPeer;
			List<object> attributes = new List<object>();

			for (int i = 0; i < peer.GetAvailablePacketCount(); i++){
				try{
					attributes.Add(BitConverter.ToInt16(peer.GetPacket(), 0));
				} catch (System.ArgumentException e){
					GD.Print(e);
				}
			}

			SetAttributes(attributes.ToArray());

			mutex = new System.Threading.Mutex();

			GenerateWorld();

			GetNode("Area").Connect("input_event", this, nameof(OnAreaInputEvent));
			SetCollisionLayerBit(20, true);

			GenerateCollisionArea();
			playerlevel = ((PlayerLevel) GetNode("../PlayerLevel"));
			playerlevel.init(cell_size, width, length);
			return;
		}

		RandomNumberGenerator randomizer = new RandomNumberGenerator();
		randomizer.Randomize();
		open_simplex_new.Seed = (int) randomizer.Randi();
		//open_symplex_new.octaves = 4
		//open_symplex_new.period = 256
		//open_symplex_new.lacunarity = 2
		//open_symplex_new.persistence = 0.5
		open_simplex_new.Octaves = randomizer.RandiRange(1, 9);
		open_simplex_new.Period = randomizer.RandfRange(10, 70);
		open_simplex_new.Lacunarity = randomizer.RandfRange(0.1f, 4);
		open_simplex_new.Persistence = randomizer.RandfRange(0, 0); // 0 - 0.5
		mutex = new System.Threading.Mutex();

		GenerateWorld();

		GetNode("Area").Connect("input_event", this, nameof(OnAreaInputEvent));
		SetCollisionLayerBit(20, true);

		GenerateCollisionArea();
		playerlevel = ((PlayerLevel) GetNode("../PlayerLevel"));
		playerlevel.init(cell_size, width, length);
	}

	[Master]
	private void GetAttributes(){
		object[] attributes = new object[8];
		attributes[0] = open_simplex_new.Seed;
		attributes[1] = open_simplex_new.Octaves;
		attributes[2] = open_simplex_new.Period;
		attributes[3] = open_simplex_new.Lacunarity;
		attributes[4] = open_simplex_new.Persistence;
		
		attributes[5] = width;
		attributes[6] = height;
		attributes[7] = CellSize;

		var peer = GetTree().NetworkPeer;
		foreach (dynamic attr in attributes) {
			peer.PutPacket(BitConverter.GetBytes(attr));
		}
		//RpcId(GetTree().GetRpcSenderId(), nameof(SetAttributes), new object[] {open_simplex_new.Seed, open_simplex_new.Octaves, open_simplex_new.Period, open_simplex_new.Lacunarity, open_simplex_new.Persistence, width, height, CellSize});
	}

	private void SetAttributes(object[] attributes){
		open_simplex_new.Seed = (int)attributes[0];
		open_simplex_new.Octaves = (int)attributes[1];
		open_simplex_new.Period = (float)attributes[2];
		open_simplex_new.Lacunarity = (float)attributes[3];
		open_simplex_new.Persistence = (float)attributes[4];
		
		width = (int) attributes[5];
		length = width;
		height = (float)attributes[6];
		CellSize = (Vector3)attributes[7];
	}

	private void GenerateCollisionArea()
	{
		CollisionShape shape = new CollisionShape();
		shape.Name = "center";
		BoxShape box = new BoxShape();
		box.ResourceName = "box";
		box.Extents = new Vector3(width, height*2, length) / 2;
		shape.Shape = box;
		shape.Translation = new Vector3(width, 0, length) / 2;
		GetNode("Area").AddChild(shape);

		/*
		//Visual Representation
		MeshInstance mi = new MeshInstance();
		mi.Mesh = new CubeMesh();
		mi.Translation = new Vector3(width, 0, length) / 2;
		mi.Scale = new Vector3(width, height*2, length) / 2;
		GetNode("Area").AddChild(mi);
		*/
	}

	private void GenerateQuadrant(System.Object obj)
	{
		Vector3[] fromto;
		try {
			fromto = (Vector3[]) obj;
	  	}
	  	catch (InvalidCastException) {
			GD.Print("nope");
			return;
	  	}
		var from = fromto[0];
		var to = fromto[1];

		var diff_x = (int) Math.Abs(to.x - from.x);
		var diff_z = (int) Math.Abs(to.z - from.z);

		foreach (int x in Enumerable.Range((int)from.x, diff_x))
		{
			foreach (int z in Enumerable.Range((int)from.z, diff_z))
			{
				int[] index_height = GetTileIndex(open_simplex_new.GetNoise3d(x, 0, z));
				bool isTree = (open_simplex_new.GetNoise3d(x, 100, z)) <= tree_spread;
				mutex.WaitOne();
				base.SetCellItem(x, index_height[1], z, index_height[0]);
				if (isTree){
					base.SetCellItem(x, index_height[1]+1, z, 20);
				}
				mutex.ReleaseMutex();
			}
		}
	}

	private void GenerateWorld()
	{
		var thread = new List<System.Threading.Thread>();
		var index = 0;

		foreach (var chunk in Enumerable.Range(1, chunk_number*chunk_number))
			thread.Add(new System.Threading.Thread(GenerateQuadrant));

		foreach (int x in Enumerable.Range(0, chunk_number))
		{
			foreach (int z in Enumerable.Range(0, chunk_number))
			{
				var from = new Vector3(x, 0, z)*chunk_loader;
				var to = new Vector3(x+1, 0, z+1)*chunk_loader;
				Vector3[] fromto = {from, to};
				thread[index++].Start(fromto);
			}
		}

		foreach (var threaddy in thread)
			threaddy.Join();

		EmitSignal(nameof(ReadySignal));
	}

	private int[] GetTileIndex(float noise_sample)
	{	
		float ground_level = (Math.Abs((Hill_Fatness)+1)/3);
		float diff = 1/((Math.Abs(Hill_Fatness-1))/Hill_Tallness);
		float f = noise_sample;
		if (f>=Hill_Fatness){
			return new int[] {27, (int)(Math.Abs(Hill_Fatness-f)*diff)};
		}	
		else{
			switch (f){
				case float noice when noice < ((ground_level*1)-1): // water
					return new int[] {1, 0}; // index   
				case float noice when noice < ((ground_level*2)-1): // sand
					return new int[] {2, 0}; // index  
				case float noice when noice < ((ground_level*3)-1): // grass
					return new int[] {36, 0}; // index  
				default:
					return new int[] {0, 0};
			}
		}
	}

	[RemoteSync]
	private void SetCellItem(int x, int y, int z, int itemIndex){
		base.SetCellItem(x,y,z,itemIndex);
	}

	public void OnAreaInputEvent(Camera camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
	{
		if (@event is InputEventMouseButton)
		{
			var mouse_event = (InputEventMouseButton) @event;
			if (mouse_event.ButtonIndex == (int) ButtonList.Left)
			{
				if (mouse_event.IsPressed())
				{
					GD.Print(click_position);
					
					pos1 = click_position/cell_size;
				}
				else
				{
					pos2 = click_position/cell_size;
					foreach (int x in Enumerable.Range(0, (int) Math.Abs(pos2.x-pos1.x)+1))
					{
						foreach (int z in Enumerable.Range(0, (int)Math.Abs(pos2.z-pos1.z)+1))
						{
							if (pos1.x < pos2.x)
							{
								if (pos1.z < pos2.z){
									SetCellItem((int)(x+pos1.x), (int)pos1.y, (int)(z+pos1.z), 26);
								}else{
									SetCellItem((int)(x+pos1.x), (int)pos1.y, (int)(z+pos2.z), 26);
								}
							}
							else
							{
								if (pos1.z < pos2.z){
									SetCellItem((int)(x+pos2.x), (int)pos1.y, (int)(z+pos1.z), 26);
								}else{
									SetCellItem((int)(x+pos2.x), (int)pos1.y, (int)(z+pos2.z), 26);
								}	
							}
						}
					}
				}
			}
			if (mouse_event.ButtonIndex == (int) ButtonList.Right && mouse_event.IsPressed()){
				playerlevel.CheckSpace(click_position);
			} 
		}
	}

	private const float rayLength = 1000;

	int placement_height_offset = 0;
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == 1)
		{
	   	 	var camera = (Camera)GetNode("../Spatial/Camera");
			var from = camera.ProjectRayOrigin(eventMouseButton.Position);
			var to = from + camera.ProjectRayNormal(eventMouseButton.Position) * rayLength;
			var spaceState = GetWorld().DirectSpaceState;
			var res = spaceState.IntersectRay(from, to, null, 0b10000000000000000000, true, true);
			
			if	(res.Contains("position")){
				GD.Print(res["position"], res["collider"]);
				Vector3 pos = (Vector3) res["position"];
				pos /= cell_size;
				if (GetTree().NetworkPeer != null && GetTree().NetworkPeer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected){
					Rpc("SetCellItem", new object[] {(int)pos.x, (int)pos.y + placement_height_offset, (int)pos.z, 26});
				} else {
					SetCellItem((int)pos.x, (int)pos.y + placement_height_offset, (int)pos.z, 26);
				}
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event){
		if (@event is InputEventKey nnn && nnn.Pressed){
			if (nnn.Scancode == (int) KeyList.E){
				placement_height_offset += 1;
			}
			else if (nnn.Scancode == (int) KeyList.Q){
				placement_height_offset -= 1;
			}
		}
	}
}
