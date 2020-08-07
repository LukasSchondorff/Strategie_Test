using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class MapGen : GridMap
{
	[Export(PropertyHint.Range, "max 10")]
	private int chunk_number = 5;
	int chunk_loader = 32;
	int width;
	int height;
	int length;
	OpenSimplexNoise open_simplex_new;
	float colision_area_factor = 1.5f;
	Vector3 pos1;
	Vector3 pos2;
	System.Threading.Mutex mutex;

	public override void _Ready()
	{
		width = chunk_number*chunk_loader;
		length = width;
		height = 1;

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
		open_simplex_new.Persistence = randomizer.RandfRange(0, 1);
		mutex = new System.Threading.Mutex();
		GenerateWorld();
		GetNode("Area").Connect("input_event", this, nameof(OnAreaInputEvent));
		//GenerateCollisionArea()
	}


	private void GenerateCollisionArea()
	{
		//((CollisionShape) GetNode("Area/CollisionShape")).Transform.origin = new Vector3(width*colision_area_factor, height*colision_area_factor, length*colision_area_factor);
		//((CollisionShape) GetNode("Area/CollisionShape")).Transform.Scaled(new Vector3(width*colision_area_factor, height*colision_area_factor, length*colision_area_factor))
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

		foreach (int x in Enumerable.Range((int)from.x, (int)to.x))
		{
			foreach (int z in Enumerable.Range((int)from.z, (int)to.z))
			{
				int index = GetTileIndex(open_simplex_new.GetNoise3d(x, 0, z));
				mutex.WaitOne();
				SetCellItem(x, 0, z, index);
				mutex.ReleaseMutex();
			}
		}
	}

	private void GenerateWorld()
	{
		var thread = new List<System.Threading.Thread>();
		var index = 0;

		foreach (var chunk in Enumerable.Range(0, chunk_number*chunk_number))
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
	}


	private int GetTileIndex(float noise_sample)
	{
		switch (noise_sample)
		{
			case float f when f < -0.5f:
				return 0; // index

			case float f when f < 0f:
				return 1; // index

			case float f when f < 0.5f:
				return 46; // index

			case float f when f < 1f:
				return 27; // index

			default:
				return 0;
		}
	}


	public void OnAreaInputEvent(Camera camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
	{
		if (@event is InputEventMouseButton)
		{
			var mouse_event = (InputEventMouseButton) @event;
			if (mouse_event.ButtonIndex == (int) ButtonList.Left && mouse_event.IsPressed())
			{
				GD.Print(click_position);
				pos1 = click_position/3;
			}
			if (mouse_event.ButtonIndex == (int) ButtonList.Left && !mouse_event.IsPressed())
			{
				GD.Print(click_position);
				pos2 = click_position/3;
				foreach (int x in Enumerable.Range(0, (int) Math.Abs(pos2.x-pos1.x)+1))
				{
					foreach (int z in Enumerable.Range(0, (int)Math.Abs(pos2.z-pos1.z)+1))
					{
						//GetCellItem(x + pos1.x,0,z + pos1.z);
						if (pos1.x < pos2.x)
						{
							if (pos1.z < pos2.z)
								SetCellItem((int)(x+pos1.x), 0, (int)(z+pos1.z) + 1, 32);
							else
								SetCellItem((int)(x+pos1.x), 0, (int)(z+pos2.z) + 1, 32);
						} 
						else
						{
							if (pos1.z < pos2.z)
								SetCellItem((int)(x+pos2.x), 0, (int)(z+pos1.z) + 1, 32);
							else
								SetCellItem((int)(x+pos2.x), 0, (int)(z+pos2.z) + 1, 32);
						}
					}
				}
			}
			if (mouse_event.ButtonIndex == (int) ButtonList.Right && mouse_event.IsPressed())
				SetCellItem((int)(click_position[0]/3), 0, (int)(click_position[2]/3) + 1, 32);

		}
	}
}
