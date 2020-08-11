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
	protected int width;
	protected float height;
	protected int length;
	OpenSimplexNoise open_simplex_new;
	float colision_area_factor = 1.5f;
	Vector3 pos1;
	Vector3 pos2;
	System.Threading.Mutex mutex;
	
	private PlayerLevel playerlevel;

	[Signal]
	public delegate void ReadySignal();	
	[Signal]
	public delegate void AttributesReceived();

	[Signal]
	public delegate void BuildingPlaced();
	public override void _Ready()
	{
		width = chunk_number*chunk_loader*(int)CellSize.x;
		length = width;
		height = 1f;
		open_simplex_new = new OpenSimplexNoise();
		mutex = new System.Threading.Mutex();
		CellSize = cell_size;

		if (GetTree().NetworkPeer != null && GetTree().NetworkPeer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected){
			if(IsNetworkMaster()){
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
			}
			else {
				RpcId(1, nameof(GetAttributes));
				WaitForAttributes();
				return;
			}
		}

		Init();
	}

	private void Init(){
		GenerateWorld();

		SetCollisionLayerBit(20, true);

		//GenerateCollisionArea();
		//GetNode("Area").Connect("input_event", this, nameof(OnAreaInputEvent));
		playerlevel = new PlayerLevel(cell_size, width, length, (GridMap)GetNode("../PlayerLevel"));
	}

	private async void WaitForAttributes(){
		await ToSignal(this, nameof(AttributesReceived));

		Init();
	}

	[Remote]
	private void GetAttributes(){
		RpcId(GetTree().GetRpcSenderId(), nameof(SetAttributes), open_simplex_new.Seed, open_simplex_new.Octaves, open_simplex_new.Period, open_simplex_new.Lacunarity, open_simplex_new.Persistence, width, height, CellSize, tree_spread);
	}

	[Remote]
	private void SetAttributes(int seed, int octaves, float period, float lacunarity, float persistence, int width, float height, Vector3 CellSize, float tree_spread){
		open_simplex_new.Seed = seed;
		open_simplex_new.Octaves = octaves;
		open_simplex_new.Period = period;
		open_simplex_new.Lacunarity = lacunarity;
		open_simplex_new.Persistence = persistence;
		
		this.width = width;
		length = width;
		this.height = height;
		this.CellSize = CellSize;
		this.tree_spread = tree_spread;

		EmitSignal(nameof(AttributesReceived));
	}

	/* now handled by static bodies on meshes
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

		//Visual Representation
		MeshInstance mi = new MeshInstance();
		mi.Mesh = new CubeMesh();
		mi.Translation = new Vector3(width, 0, length) / 2;
		mi.Scale = new Vector3(width, height*2, length) / 2;
		GetNode("Area").AddChild(mi);
	}
	*/

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

				mutex.WaitOne();
				float interval = 1/(float)Hill_Tallness * ((Hill_Fatness < 0) ? ((Hill_Fatness+1)/2) : Math.Abs((Hill_Fatness-1)/2));

				int extra_blocks = (int) (Math.Abs(open_simplex_new.GetNoise3d(x, 0, z) - open_simplex_new.GetNoise3d(x+1, 0, z)) / interval);
				int extra_blocks1 = (int) (Math.Abs(open_simplex_new.GetNoise3d(x, 0, z) - open_simplex_new.GetNoise3d(x-1, 0, z)) / interval);
				int extra_blocks2 = (int) (Math.Abs(open_simplex_new.GetNoise3d(x, 0, z) - open_simplex_new.GetNoise3d(x, 0, z+1)) / interval);
				int extra_blocks3 = (int) (Math.Abs(open_simplex_new.GetNoise3d(x, 0, z) - open_simplex_new.GetNoise3d(x, 0, z-1)) / interval);

				//GD.Print(extra_blocks, extra_blocks1, extra_blocks2, extra_blocks3);

				int max = 0;
				if(extra_blocks > extra_blocks1)
					if(extra_blocks > extra_blocks2)
						if(extra_blocks > extra_blocks3)
							max = extra_blocks;
						else
							max = extra_blocks3;
					else
						if(extra_blocks2 > extra_blocks3)
							max = extra_blocks2;
						else
							max = extra_blocks3;
				else
					if(extra_blocks1 > extra_blocks2)
						if(extra_blocks1 > extra_blocks3)
							max = extra_blocks1;
						else
							max = extra_blocks3;
					else
						if(extra_blocks2 > extra_blocks3)
							max = extra_blocks2;
						else
							max = extra_blocks3;

				bool isTree = (open_simplex_new.GetNoise3d(x, 100, z)) <= tree_spread;
				base.SetCellItem(x, index_height[1], z, index_height[0]);
				
				for (int y = 1; y < max; y++){
					if(index_height[1]-y < 0)
						break;
					base.SetCellItem(x, index_height[1]-y, z, index_height[0]);
				}
				
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
		
		if (IsNetworkMaster())
			GetNode("/root/MapSync").EmitSignal("ReadyToSendAttributes");
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
	public void SetCellItem(int x, int y, int z, int itemIndex){
		base.SetCellItem(x,y,z,itemIndex);
	}

	/* now handled by static body on meshes
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
				//playerlevel.CheckSpace(click_position);
			} 
		}
	}
	*/

	private const float rayLength = 1000;

	float placement_height_offset = 0f;
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.ButtonIndex == (int) ButtonList.Left)
			{
				var camera = (Camera)GetNode("../Spatial/Camera");
				var from = camera.ProjectRayOrigin(eventMouseButton.Position);
				var to = from + camera.ProjectRayNormal(eventMouseButton.Position) * rayLength;
				var spaceState = GetWorld().DirectSpaceState;
				var res = spaceState.IntersectRay(from, to, null, 0b10000000000000000000, true, true);
				Vector3 click_position = new Vector3(0, placement_height_offset, 0);

				if	(res.Contains("position")){
					GD.Print(res["position"], res["collider"]);
					click_position += (Vector3) res["position"];
					if (eventMouseButton.IsPressed())
					{
						//GD.Print(click_position);
						
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
										Rpc(nameof(SetCellItem), (int)(x+pos1.x), (int)pos1.y, (int)(z+pos1.z), 26);
									}else{
										Rpc(nameof(SetCellItem), (int)(x+pos1.x), (int)pos1.y, (int)(z+pos2.z), 26);
									}
								}
								else
								{
									if (pos1.z < pos2.z){
										Rpc(nameof(SetCellItem), (int)(x+pos2.x), (int)pos1.y, (int)(z+pos1.z), 26);
									}else{
										Rpc(nameof(SetCellItem), (int)(x+pos2.x), (int)pos1.y, (int)(z+pos2.z), 26);
									}	
								}
							}
						}
					}
				}
			}
			if (eventMouseButton.ButtonIndex == (int) ButtonList.Right && eventMouseButton.IsPressed()){
				var camera = (Camera)GetNode("../Spatial/Camera");
				var from = camera.ProjectRayOrigin(eventMouseButton.Position);
				var to = from + camera.ProjectRayNormal(eventMouseButton.Position) * rayLength;
				var spaceState = GetWorld().DirectSpaceState;
				var res = spaceState.IntersectRay(from, to, null, 0b10000000000000000000, true, true);
				Vector3 click_position = new Vector3(0, placement_height_offset, 0);

				if	(res.Contains("position")){
					//GD.Print(res["position"], res["collider"]);
					click_position += (Vector3) res["position"];
				}
				playerlevel.getBuildingLevel().CheckSpace(click_position);
				EmitSignal(nameof(BuildingPlaced), click_position);
			} 
		}
	}

	public override void _UnhandledInput(InputEvent @event){
		if (@event is InputEventKey nnn && nnn.Pressed){
			if (nnn.Scancode == (int) KeyList.E){
				placement_height_offset += cell_size.y;
			}
			else if (nnn.Scancode == (int) KeyList.Q){
				placement_height_offset -= cell_size.y;
			}
		}
	}
	
	
	//TODO auslagern 
	public PlayerLevel GetPlayerLevel() {
		return playerlevel;
	}
}
