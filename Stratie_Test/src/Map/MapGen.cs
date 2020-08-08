using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class MapGen : GridMap
{
	[Export(PropertyHint.Range, "0,15,or_greater")]
	private int chunk_number = 5;
	
	[Export(PropertyHint.Range, "-1,1,0.01")]
	private float Hill_Fatness = -0.5f;
	
	[Export(PropertyHint.Range, "1,100,or_greater")]
	private int Hill_Tallness = 100;
	
	private List <Area> arealevel;
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

		arealevel = new List <Area>();

			
			
			


		Godot.Collections.Array test = GetMeshes();

		CellSize = cell_size;

		RandomNumberGenerator randomizer = new RandomNumberGenerator();
		randomizer.Randomize();
		open_simplex_new = new OpenSimplexNoise();
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
		GenerateCollisionArea();
		playerlevel = ((PlayerLevel) GetNode("../PlayerLevel"));
		playerlevel.init(cell_size, width, length);
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
				mutex.WaitOne();
				SetCellItem(x, index_height[1], z, index_height[0], 10);
				Area temp_area = new Area();
				CollisionShape shape = new CollisionShape();
				shape.Shape = new BoxShape();
				shape.Translation = new Vector3(x*cell_size.x + cell_size.x/2, index_height[1]*cell_size.y, z*cell_size.z + cell_size.z/2);
				shape.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);temp_area.Monitoring = false;
				temp_area.CollisionMask = 0;
				temp_area.CollisionLayer = 0b10000000000000000000;
				temp_area.AddChild(shape);
				arealevel.Add(temp_area);
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

		foreach (Area area in arealevel){
			AddChild(area);
			area.Connect("input_event", this, nameof(OnAreaInputEvent));
			area.Connect("area_entered", this, nameof(Collision_DC));
		}
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
					//GD.Print(click_position);
					pos2 = click_position/cell_size;
					foreach (int x in Enumerable.Range(0, (int) Math.Abs(pos2.x-pos1.x)+1))
					{
						foreach (int z in Enumerable.Range(0, (int)Math.Abs(pos2.z-pos1.z)+1))
						{
							//GetCellItem(x + pos1.x,0,z + pos1.z);
							if (pos1.x < pos2.x)
							{
								if (pos1.z < pos2.z)
									SetCellItem((int)(x+pos1.x), (int)pos1.y, (int)(z+pos1.z), 26);
								else
									SetCellItem((int)(x+pos1.x), (int)pos1.y, (int)(z+pos2.z), 26);
							}
							else
							{
								if (pos1.z < pos2.z)
									SetCellItem((int)(x+pos2.x), (int)pos1.y, (int)(z+pos1.z), 26);
								else
									SetCellItem((int)(x+pos2.x), (int)pos1.y, (int)(z+pos2.z), 26);
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
	private void Collision_DC(Area area){
		GD.Print(area);
	}
}
