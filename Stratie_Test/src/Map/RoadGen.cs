using Godot;
using System;
using System.Collections.Generic;

public class RoadGen : MapGen
{
	private List<Vector3> buildingLocations;

	private Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Vector3>>> buildingLocations1;
	
	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int itemIndex){
		((GridMap) GetNode("../")).SetCellItem(x,y,z,itemIndex);
	}
	
	public override void _Ready()
	{
		buildingLocations = new List<Vector3>();
		buildingLocations1 = new Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Vector3>>>();
		Connect("BuildingPlaced", this, nameof(AddBuilding));

		base._Ready();
	}

	private void AddBuilding(Vector3 pos) {
		pos /= CellSize;

		buildingLocations.Add(pos);
		if(buildingLocations.Count >= 2){
			Vector3 nearestBuilding = new Vector3();
			float closestDistance = width*length*height+1;
			foreach(Vector3 other in buildingLocations){
				if(other != pos){
					if(pos.DistanceTo(other) < closestDistance){
					 	closestDistance = pos.DistanceTo(other);
						nearestBuilding = other;	
					}
				}
			}
			GenerateRoad(buildingLocations[buildingLocations.Count-1], nearestBuilding);
		}
	}

	private void GenerateRoad(Vector3 pos1, Vector3 pos2){
		
		int x = 0;
		int localOrientation = 0;
		if(pos1.x < pos2.x){
			if(pos1.z < pos2.z){
				localOrientation = 10;
			}
			else{
				localOrientation = 16;
			}
		}
		else{
			if(pos1.z < pos2.z){
				localOrientation = 22;
			}
			else{
				localOrientation = 0;
			}
		}
		if (pos1.x < pos2.x){
			for (; x < Math.Abs((int)pos1.x - (int)pos2.x); x++){
				SetRoadTile((int)pos1.x+x, (int)pos1.y, (int)pos1.z, 0);
			}
		}
		else {
			for (; x > -Math.Abs((int)pos1.x - (int)pos2.x); x--){
				SetRoadTile((int)pos1.x+x, (int)pos1.y, (int)pos1.z, 0);
			}
		}
		RandomNumberGenerator rand = new RandomNumberGenerator();
		rand.Randomize();

		switch(localOrientation){
			case 0:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 10:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 22:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 16:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if (GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
		}

		if (pos1.z < pos2.z){
			for (int z = 1; z < Math.Abs((int)pos1.z - (int)pos2.z); z++){
				SetRoadTile((int)pos1.x+x, (int)pos1.y, (int)pos1.z+z, 16);
			}
		}
		else {
			for (int z = -1; z > -Math.Abs((int)pos1.z - (int)pos2.z); z--){
				SetRoadTile((int)pos1.x+x, (int)pos1.y, (int)pos1.z+z, 16);
			}
		}
	}
	
	public new void SetCellItem(int x, int y, int z, int item, int rot){
		if(GetCellItem(x,y,z) != 14){
			base.SetCellItem(x,y,z,item,rot);
		}
	}

	private void SetRoadTile(int x, int y, int z, int rot){
		switch(GetCellItem(x, y, z)){
			case 0:
				if(GetCellItemOrientation(x, y, z) != rot){
					SetCellItem(x, y, z, 14, rot);
				}
				else {
					SetCellItem(x, y, z, 0, rot);
				}
				break;
			case 16:
			case 22:
				if(rot == 16){
					if(GetCellItemOrientation(x,y,z) == 0 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, 13, 0);
					}
					else{
						SetCellItem(x,y,z, 13, 10);
					}
				}
				else{
					if(GetCellItemOrientation(x,y,z) == 10 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, 13, 22);
					}
					else{
						SetCellItem(x,y,z, 13, 16);
					}
				}
				break;
			case 13:
				if(rot == 16){
					if(GetCellItemOrientation(x,y,z) == 16 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, 14, 0);
					}
				}
				else{
					if(GetCellItemOrientation(x,y,z) == 0 || GetCellItemOrientation(x,y,z) == 10){
						SetCellItem(x,y,z, 14, 0);
					}
				}
				break;
			default:
				SetCellItem(x,y,z,0,rot);
				break;
		}
	}

	private void SetCornerOnRoad(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, 13, 0);
				}
				else{
					SetCellItem(x, y, z, 13, 16);
				}
				break;
			case 10:
				if(GetCellItemOrientation(+x, y, z) == 16){
					SetCellItem(x, y, z, 13, 10);
				}
				else{
					SetCellItem(x, y, z, 13, 22);
				}
				break;
			case 22:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, 13, 0);
				}
				else{
					SetCellItem(x, y, z, 13, 22);
				}
				break;
			case 16:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, 13, 10);
				}
				else{
					SetCellItem(x, y, z, 13, 16);
				}
				break;
		}
	}

	private void SetCornerOnT(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				if(GetCellItemOrientation(x, y, z) == 10 || GetCellItemOrientation(x, y, z) == 22){
					SetCellItem(x, y, z, 14, 0);
				}
				break;
			case 10:
				if(GetCellItemOrientation(x, y, z) == 0 || GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, 14, 0);
				}
				break;
			case 22:
				if(GetCellItemOrientation(x, y, z) == 16 || GetCellItemOrientation(x, y, z) == 10){
					SetCellItem(x, y, z, 14, 0);
				}
				break;
			case 16:
				if(GetCellItemOrientation(x, y, z) == 0 || GetCellItemOrientation(x, y, z) == 22){
					SetCellItem(x, y, z, 14, 0);
				}
				break;
		}
	}

	private void SetCornerOnCorner(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				switch(GetCellItemOrientation(x, y, z)){
					case 16:
						SetCellItem(x, y, z, 13, 16);
						break;
					case 22:
						SetCellItem(x, y, z, 13);
						break;
					case 10:
						SetCellItem(x, y, z, 14);
						break;
				}
				break;
			case 10:
				switch(GetCellItemOrientation(x, y, z)){
					case 16:
						SetCellItem(x, y, z, 13, 10);
						break;
					case 22:
						SetCellItem(x, y, z, 13, 22);
						break;
					case 0:
						SetCellItem(x, y, z, 14);
						break;
				}
				break;
			case 22:
				switch(GetCellItemOrientation(x, y, z)){
					case 10:
						SetCellItem(x, y, z, 13);
						break;
					case 0:
						SetCellItem(x, y, z, 13, 16);
						break;
					case 16:
						SetCellItem(x, y, z, 14);
						break;
				}
				break;
			case 16:
				switch(GetCellItemOrientation(x, y, z)){
					case 0:
						SetCellItem(x, y, z, 13, 16);
						break;
					case 10:
						SetCellItem(x, y, z, 13, 10);
						break;
					case 22:
						SetCellItem(x, y, z, 14);
						break;
				}
				break;
		}
	}
}
