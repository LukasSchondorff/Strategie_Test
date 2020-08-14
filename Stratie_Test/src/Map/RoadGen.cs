using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class RoadGen : MapGen
{
	private List<Vector3> buildingLocations;

	private Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int, Vector3>>> buildingLocations1;
	
	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int itemIndex){
		((GridMap) GetNode("./")).SetCellItem(x,y,z,itemIndex);
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
			Vector3 nearestBuilding = GetNearestBuilding(pos);

			Vector3 distances = new Vector3(Math.Abs((int)nearestBuilding.x-(int)pos.x), Math.Abs((int)nearestBuilding.y-(int)pos.y), Math.Abs((int)nearestBuilding.z-(int)pos.z));

			int road_length = (int)distances.x + (int)distances.z;
			int road_height = (int)distances.y;

			GD.Print(distances);

			if(road_height != 0){

				if(road_length == 0) {
					return;
				}
				float slope_percentage =  road_height / road_length;

				if(slope_percentage > 0.5f){
					// TODO: Too steep
				}
				else{
					RandomNumberGenerator randomizer = new RandomNumberGenerator();
					List<int> roadTiles = new List<int>();
					if(distances.x > 0){
						roadTiles.AddRange(Enumerable.Range(0, (int)(distances.x))); // -2 for building and corner
					}
					if(distances.z > 0){
						roadTiles.AddRange(Enumerable.Range((int)(distances.x+1), (int)(distances.z)-1)); // -2 for building and corner
					}

					List<int> roadTiles_with_rotation = new List<int>();
					for(int y = 0; y < road_height; y++){
						int rand = randomizer.RandiRange(0, roadTiles.Count-1);
						int tmp = roadTiles[rand];
						roadTiles.Remove(tmp);

						roadTiles_with_rotation.Add(tmp);
					}

					GD.Print(road_height);					

					GenerateRoad(buildingLocations[buildingLocations.Count-1], nearestBuilding, roadTiles_with_rotation);
				}
			}
			else{
				GenerateRoad(buildingLocations[buildingLocations.Count-1], nearestBuilding, new List<int>{});
			}
		}
		
	}

	private void GenerateRoad(Vector3 pos1, Vector3 pos2, List<int> additations){
		int x = 1;
		int y = 0;
		int roadIndex = 0;
		GD.Print(additations.Count);
		bool up = false;

		int localOrientation = 0;
		int slope_xOrientation = 0;
		int slope_zOrientation = 0;

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

		if(pos1.x <= pos2.x){
			if(pos1.z <= pos2.z){
				if(pos1.y < pos2.y){
					slope_xOrientation = 10;
					slope_zOrientation = 16;
					up = true;
				}
				else {
					slope_xOrientation = 0;
					slope_zOrientation = 22;
					up = false;
				}
			}
			else{
				if(pos1.y < pos2.y){
					slope_xOrientation = 10;
					slope_zOrientation = 22;
					up = true;
				}
				else {
					slope_xOrientation = 0;
					slope_zOrientation = 16;
					up = false;
				}
			}
		}
		else{
			if(pos1.z <= pos2.z){
				if(pos1.y < pos2.y){
					slope_xOrientation = 0;
					slope_zOrientation = 16;
					up = true;
				}
				else {
					slope_xOrientation = 10;
					slope_zOrientation = 22;
					up = false;
				}
			}
			else{
				if(pos1.y < pos2.y){
					slope_xOrientation = 0;
					slope_zOrientation = 22;
					up = true;
				}
				else {
					slope_xOrientation = 10;
					slope_zOrientation = 16;
					up = false;
				}				
			}
		}

		if (pos1.x < pos2.x){
			for (; x < Math.Abs((int)pos1.x - (int)pos2.x); x++){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, 0);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y--), (int)pos1.z, slope_xOrientation);
				}
			}
		}
		else {
			for (x = -1; x > -Math.Abs((int)pos1.x - (int)pos2.x); x--){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, 0);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y--), (int)pos1.z, slope_xOrientation);
				}
			}
		}
		RandomNumberGenerator rand = new RandomNumberGenerator();
		rand.Randomize();

		roadIndex++;

		switch(localOrientation){
			case 0:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 10:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 22:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
			case 16:
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
					SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if (GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 13){
					SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 16 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 22){
					SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
					break;
				}
				else {
					SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? 22:16, localOrientation);
					break;
				}
		}

		if (pos1.z < pos2.z){
			for (int z = 1; z < Math.Abs((int)pos1.z - (int)pos2.z); z++){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z, 16);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}
		else {
			for (int z = -1; z > -Math.Abs((int)pos1.z - (int)pos2.z); z--){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z, 16);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}

		GD.Print(roadIndex);
	}
	
	public new void SetCellItem(int x, int y, int z, int item, int rot){
		if(GetCellItem(x,y,z) != 14){
			base.SetCellItem(x,y,z,item,rot);

			if(item == 51){
				for(int i = 1; i <= 2; i++){
					base.SetCellItem(x,y+i,z,-1,0);
				}
			}
			else {
				base.SetCellItem(x,y+1,z,-1,0);
			}
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

	private void SetSlopeTile(int x, int y, int z, int rot){
		SetCellItem(x, y, z, 51, rot);
	}

	private Vector3 GetNearestBuilding(Vector3 pos){
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
		return nearestBuilding;
	}
}
