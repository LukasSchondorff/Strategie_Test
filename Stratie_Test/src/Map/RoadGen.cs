using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class RoadGen : MapGen
{
	private bool zitter_gleich = false;
	private static class CellItem{
		public const int StraightRoad = 0;
		public const int Building = 49;
		public const int Slope = 51;
		public const int Corner1 = 16; 
		public const int Corner2 = 22;
		public const int Slope_Arch_OverKopf = 50;
		public const int Pillar = 7;
		public const int T = 13;
		public const int Crossing = 14;
	}

	private List<Vector3> road_path;

	private List<Vector3> buildingLocations;

	private List<Vector3> roadLocations;
	
	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int itemIndex){
		((GridMap) GetNode("./")).SetCellItem(x, y, z, itemIndex);
	}
	
	public override void _Ready()
	{
		buildingLocations = new List<Vector3>();
		roadLocations = new List<Vector3>();
		road_path = new List<Vector3>();
		Connect("BuildingPlaced", this, nameof(AddBuilding));

		base._Ready();
	}

	public bool AddBuilding(Vector3 pos) {
		pos /= CellSize;
		pos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
		if (GetCellItem((int)pos.x, (int)pos.y, (int)pos.z) == CellItem.StraightRoad){
			buildingLocations.Add(pos);
			return true;
		}else {
			for (int i = -2; i < 200; i++)
				if (GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.StraightRoad || GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Building
					|| GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Slope || GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Corner1
					|| GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Corner2){
					return false;
				}
		}
		if(buildingLocations.Count >= 1){
			Vector3 nearestBuilding = GetNearestBuilding(pos);
			Vector3 nearestRoad = GetNearestRoad(pos);

			Vector3 distances = new Vector3(Math.Abs((int)nearestBuilding.x-(int)pos.x), Math.Abs((int)nearestBuilding.y-(int)pos.y), Math.Abs((int)nearestBuilding.z-(int)pos.z));

			if (nearestRoad != Vector3.Zero && pos.DistanceTo(nearestRoad) < pos.DistanceTo(nearestBuilding)){
				distances = new Vector3(Math.Abs((int)nearestRoad.x-(int)pos.x), Math.Abs((int)nearestRoad.y-(int)pos.y), Math.Abs((int)nearestRoad.z-(int)pos.z));
			}

			int road_length = (int)distances.x + (int)distances.z;
			int road_height = (int)distances.y;

			if(road_height != 0){

				if(road_length == 0) {
					return false;
				}
				float slope_percentage =  road_height / road_length;

				if(slope_percentage > 0.5f){
					return false;
					// TODO: Too steep
				}
				else{
					RandomNumberGenerator randomizer = new RandomNumberGenerator();
					List<int> roadTiles = new List<int>();
					if(distances.x > 0){
						roadTiles.AddRange(Enumerable.Range(0, (int)(distances.x)-1)); 

						if(distances.z > 1){
							roadTiles.AddRange(Enumerable.Range((int)(distances.x+1), (int)(distances.z)-2)); 
						}
					}
					else{
						if(distances.z > 0){
							roadTiles.AddRange(Enumerable.Range((int)(distances.x), (int)(distances.z)-1)); 
						}
					}

					List<int> roadTiles_with_rotation = new List<int>();
					for(int y = 0; y < road_height; y++){
						if(roadTiles.Count == 0){
							return false;
						}
						int rand = randomizer.RandiRange(0, roadTiles.Count-1);
						int tmp = roadTiles[rand];
						roadTiles.Remove(tmp);

						roadTiles_with_rotation.Add(tmp);
					}				

					buildingLocations.Add(pos);
					if (nearestRoad != Vector3.Zero && pos.DistanceTo(nearestRoad) < pos.DistanceTo(nearestBuilding)){
						GenerateRoad(nearestRoad, buildingLocations[buildingLocations.Count-1], roadTiles_with_rotation);
						zitter_gleich = true;
					}
					else {
						GenerateRoad(buildingLocations[buildingLocations.Count-1], nearestBuilding, roadTiles_with_rotation);
						zitter_gleich = false;
					}
					return true;
				}
			}
			else{
				buildingLocations.Add(pos);
				if (nearestRoad != Vector3.Zero && pos.DistanceTo(nearestRoad) < pos.DistanceTo(nearestBuilding)){
					GenerateRoad(nearestRoad, buildingLocations[buildingLocations.Count-1], new List<int>());
					zitter_gleich = true;
				}
				else {
					GenerateRoad(buildingLocations[buildingLocations.Count-1], nearestBuilding, new List<int>());
					zitter_gleich = false;
				}
				return true;
			}
		}
		else{
			buildingLocations.Add(pos);
			return true;
		}
	}

	public bool ValidBuildingLocation(Vector3 pos){
		pos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
		if (GetCellItem((int)pos.x, (int)pos.y, (int)pos.z) == CellItem.StraightRoad){
			return true;
		}else {
			for (int i = -2; i < 200; i++)
				if (GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.StraightRoad || GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Building
					|| GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Slope || GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Corner1
					|| GetCellItem((int)pos.x, (int)pos.y+i, (int)pos.z) == CellItem.Corner2){
					return false;
				}
		}
		if(buildingLocations.Count >= 1){
			Vector3 nearestBuilding = GetNearestBuilding(pos);
			Vector3 nearestRoad = GetNearestRoad(pos);

			Vector3 distances = new Vector3(Math.Abs((int)nearestBuilding.x-(int)pos.x), Math.Abs((int)nearestBuilding.y-(int)pos.y), Math.Abs((int)nearestBuilding.z-(int)pos.z));

			if (nearestRoad != Vector3.Zero && pos.DistanceTo(nearestRoad) < pos.DistanceTo(nearestBuilding)){
				distances = new Vector3(Math.Abs((int)nearestRoad.x-(int)pos.x), Math.Abs((int)nearestRoad.y-(int)pos.y), Math.Abs((int)nearestRoad.z-(int)pos.z));
			}

			int road_length = (int)distances.x + (int)distances.z;
			int road_height = (int)distances.y;

			if(road_height != 0){

				if(road_length == 0) {
					return false;
				}
				float slope_percentage =  road_height / road_length;

				if(slope_percentage > 0.5f){
					return false;
					// TODO: Too steep
				}
				else{
					RandomNumberGenerator randomizer = new RandomNumberGenerator();
					List<int> roadTiles = new List<int>();
					if(distances.x > 0){
						roadTiles.AddRange(Enumerable.Range(0, (int)(distances.x)-1)); 

						if(distances.z > 1){
							roadTiles.AddRange(Enumerable.Range((int)(distances.x+1), (int)(distances.z)-2)); 
						}
					}
					else{
						if(distances.z > 0){
							roadTiles.AddRange(Enumerable.Range((int)(distances.x), (int)(distances.z)-1)); 
						}
					}

					List<int> roadTiles_with_rotation = new List<int>();
					for(int y = 0; y < road_height; y++){
						if(roadTiles.Count == 0){
							return false;
						}
						int rand = randomizer.RandiRange(0, roadTiles.Count-1);
						int tmp = roadTiles[rand];
						roadTiles.Remove(tmp);

						roadTiles_with_rotation.Add(tmp);
					}				

					return true;
				}
			}
			else{
				return true;
			}
		}
		else{
			return true;
		}
	}
	public bool RemoveBuilding(Vector3 building){
		return buildingLocations.Remove(building);
	}

	public bool RemoveLastBuilding(){
		if(buildingLocations.Count > 0){
			buildingLocations.RemoveAt(buildingLocations.Count-1);
			return true;
		}
		return false;
	}

	private void GenerateRoad(Vector3 pos1, Vector3 pos2, List<int> additations){
		GD.Print("from: ", pos1, " | to: ", pos2);
		GD.Print("number of slopes: ", additations.Count);
		int x = (pos1.x == pos2.x)? 0 : 1;
		int y = 0;
		int z = 1;
		int roadIndex = 0;
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
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z, slope_xOrientation);
				}
			}
		}
		else {
			if(Math.Abs((int)pos1.x - (int)pos2.x) > 0){
				x = -1;
			}
			for (; x > -Math.Abs((int)pos1.x - (int)pos2.x); x--){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, 0);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z, slope_xOrientation);
				}
			}
		}
		RandomNumberGenerator rand = new RandomNumberGenerator();
		rand.Randomize();


		if(pos1.x != pos2.x && pos1.z != pos2.z){
			roadIndex++;
			switch(localOrientation){
				case 0:
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == 0){
						SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.T){
						SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner1 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner2){
						SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					else {
						SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? CellItem.Corner2:CellItem.Corner1, localOrientation);
						break;
					}
				case 10:
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.StraightRoad){
						SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.T){
						SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner1 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner2){
						SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					else {
						SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? CellItem.Corner2:CellItem.Corner1, localOrientation);
						break;
					}
				case 22:
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.StraightRoad){
						SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.T){
						SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner1 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner2){
						SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					else {
						SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? CellItem.Corner2:CellItem.Corner1, localOrientation);
						break;
					}
				case CellItem.Corner1:
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.StraightRoad){
						SetCornerOnRoad((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if (GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.T){
						SetCornerOnT((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					if(GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner1 || GetCellItem((int)pos1.x+x, (int)pos1.y, (int)pos1.z) == CellItem.Corner2){
						SetCornerOnCorner((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, localOrientation);
						break;
					}
					else {
						SetCellItem((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z, (rand.RandiRange(0, 1) == 0) ? CellItem.Corner2:CellItem.Corner1, localOrientation);
						break;
					}
			}
			
		}

		if (pos1.z < pos2.z){
			for (; z < Math.Abs((int)pos1.z - (int)pos2.z); z++){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z, CellItem.Corner1);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}
		else {
			for (z = -1; z > -Math.Abs((int)pos1.z - (int)pos2.z); z--){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z, CellItem.Corner1);
				}
				else{
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}
		SetBridgeTile();
	}

	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int item, int rot){
		
		if(GetCellItem(x,y,z) != CellItem.Crossing){
			((GridMap) GetNode("./")).SetCellItem(x, y, z, item, rot);
			roadLocations.Add(new Vector3(x, y, z));
			road_path.Add(new Vector3(x, y, z));
			if(item == CellItem.Building){
				for(int i = 1; i <= 2; i++){
					((GridMap) GetNode("./")).SetCellItem(x, y + i, z, -1, 0);
				}
			}
			else {
				((GridMap) GetNode("./")).SetCellItem(x, y + 1, z, -1, 0);
			}
		}
	}

	public void SetBridgerinoItem(int x, int y, int z, int item, int rot = 0){
		((GridMap) GetNode("./")).SetCellItem(x, y, z, item, rot);
	}
	/*
	private Vector3 TestBuildingRoadConnection(Vector3 pos1, Vector3 pos2){
		GD.Print(pos1, " | ", pos2);
		Vector3 startRoadPos = new Vector3();
		if(pos1.z < pos2.z){
			if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z-1) == CellItem.StraightRoad){
				startRoadPos = new Vector3(pos2.x, pos2.y, pos2.z-1);
			}
			else{
				if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z-1) == -1){
					if(GetCellItem((int)pos2.x, (int)pos2.y-1, (int)pos2.z-1) == CellItem.Building){
						startRoadPos = new Vector3(pos2.x, pos2.y-1, pos2.z-1);
					}
					else{
						return pos2;
					}
				}
				else{
					if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z-1) == CellItem.Building){
						startRoadPos = new Vector3(pos2.x, pos2.y, pos2.z-1);
					}
					else{
						return pos2;
					}
				}
			}
			return FollowRoadImproved((int)startRoadPos.x, (int)startRoadPos.y, (int)startRoadPos.z, true, pos1, Vector3.Zero);
		}
		else{
			if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z+1) == CellItem.StraightRoad){
				startRoadPos = new Vector3(pos2.x, pos2.y, pos2.z+1);
			}
			else{
				if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z+1) == -1){
					if(GetCellItem((int)pos2.x, (int)pos2.y-1, (int)pos2.z+1) == CellItem.Building){
						startRoadPos = new Vector3(pos2.x, pos2.y-1, pos2.z+1);
					}
					else{
						return pos2;
					}
				}
				else{
					if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z-1) == CellItem.Building){
						startRoadPos = new Vector3(pos2.x, pos2.y, pos2.z-1);
					}
					else{
						return pos2;
					}
				}
			}
			return FollowRoadImproved((int)startRoadPos.x, (int)startRoadPos.y, (int)startRoadPos.z, false, pos1, Vector3.Zero);
		}
	}

	private Vector3 FollowRoad(int x, int y, int z, bool himmelsrichtung, Vector3 orgiana){
		bool reached_x_z = (x == (int)orgiana.x || z == (int)orgiana.z);
		if (GetCellItem(x, y, z) == CellItem.Corner1 || GetCellItem(x, y, z) == CellItem.Corner1 || GetCellItem(x, y, z) == CellItem.Building){
			return new Vector3(x, y, z);
		}
		else if (GetCellItem(x, y, z) == CellItem.Building){
			switch(GetCellItemOrientation(x, y, z)){
				case 16 when himmelsrichtung:
					z = z - 1;
					y = y - 1;
					return FollowRoad(x, y, z, himmelsrichtung, orgiana);
					
				case 22 when himmelsrichtung:
					z = z - 1;
					y = y + 1;
					return FollowRoad(x, y, z, himmelsrichtung, orgiana);
					
				case 16 when !himmelsrichtung:
					z = z + 1;
					y = y + 1;
					return FollowRoad(x, y, z, himmelsrichtung, orgiana);
					
				case 22 when !himmelsrichtung: 
					z = z + 1;
					y = y - 1;
					return FollowRoad(x, y, z, himmelsrichtung, orgiana);
					
			}
		}
		else if (GetCellItem(x, y ,z) == CellItem.StraightRoad){ 
			if(!reached_x_z){
				if(himmelsrichtung){
					return FollowRoad(x, y, z-1, himmelsrichtung, orgiana);
				}else
					return FollowRoad(x, y, z+1, himmelsrichtung, orgiana);
			}
			else{
				return new Vector3(x, y, z);
			}
		}
		else {

		}
		return Vector3.Zero;
	}
	*/

	private Vector3 FollowRoadImproved(int x, int y, int z, bool himmelsrichtung, Vector3 orgiana, Vector3 lastStraightRoad){
		bool reached_z = false;
		if (himmelsrichtung){
			reached_z = z <= (int)orgiana.z;
		}
		else {
			reached_z = z >= (int)orgiana.z;
		}
		switch (GetCellItem(x, y, z)){
			case CellItem.Crossing:
			case CellItem.StraightRoad:
				if (reached_z){
					z += (himmelsrichtung)? -1 : 1;
					return new Vector3(x, y, z);
				}
				Vector3 currRoad = new Vector3(x, y, z);
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImproved(x, y, z, himmelsrichtung, orgiana, currRoad);
			case CellItem.Corner1:
			case CellItem.Corner2:
				z += (himmelsrichtung)? 1 : -1;
				return new Vector3(x, y, z);
			case CellItem.Slope:
				y++;
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImproved(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
			case -1:
				y--;
				if (GetCellItem(x, y, z) == CellItem.Building){
					z += (himmelsrichtung)? -1 : 1;
					if (FollowRoadImproved(x, y, z, himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
						return lastStraightRoad;
					}
					else {
						return FollowRoadImproved(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
					}
				}
				else{
					return Vector3.Zero;
				}
			case CellItem.Building:
				return lastStraightRoad;
			case CellItem.T:
				if (FollowRoadImproved(x, y, z + ((himmelsrichtung)? -1 : 1), himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
					z += (himmelsrichtung)? 1 : -1;
					return new Vector3(x, y, z);
				}
				if (reached_z){
					z += (himmelsrichtung)? 1 : -1;
					return new Vector3(x, y, z);
				}
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImproved(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
		}

		return Vector3.Zero;
	}

	private void SetRoadTile(int x, int y, int z, int rot){
		switch(GetCellItem(x, y, z)){
			case CellItem.StraightRoad:
				if(GetCellItemOrientation(x, y, z) != rot){
					SetCellItem(x, y, z, CellItem.Crossing, rot);
				}
				else {
					SetCellItem(x, y, z, 0, rot);
				}
				break;
			case CellItem.Corner1:
			case CellItem.Corner2:
				if(rot == 16){
					if(GetCellItemOrientation(x,y,z) == 0 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, CellItem.T, 0);
					}
					else{
						SetCellItem(x,y,z, CellItem.T, 10);
					}
				}
				else{
					if(GetCellItemOrientation(x,y,z) == 10 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, CellItem.T, 22);
					}
					else{
						SetCellItem(x,y,z, CellItem.T, 16);
					}
				}
				break;
			case CellItem.T:
				if(rot == 16){
					if(GetCellItemOrientation(x,y,z) == 16 || GetCellItemOrientation(x,y,z) == 22){
						SetCellItem(x,y,z, CellItem.Crossing, 0);
					}
				}
				else{
					if(GetCellItemOrientation(x,y,z) == 0 || GetCellItemOrientation(x,y,z) == 10){
						SetCellItem(x,y,z, CellItem.Crossing, 0);
					}
				}
				break;
			default:
				SetCellItem(x,y,z,CellItem.StraightRoad,rot);
				break;
		}
	}

	private void SetCornerOnRoad(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, CellItem.T, 0);
				}
				else{
					SetCellItem(x, y, z, CellItem.T, 16);
				}
				break;
			case 10:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, CellItem.T, 10);
				}
				else{
					SetCellItem(x, y, z, CellItem.T, 22);
				}
				break;
			case 22:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, CellItem.T, 0);
				}
				else{
					SetCellItem(x, y, z, CellItem.T, 22);
				}
				break;
			case 16:
				if(GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, CellItem.T, 10);
				}
				else{
					SetCellItem(x, y, z, CellItem.T, 16);
				}
				break;
		}
	}

	private void SetCornerOnT(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				if(GetCellItemOrientation(x, y, z) == 10 || GetCellItemOrientation(x, y, z) == 22){
					SetCellItem(x, y, z, CellItem.Crossing, 0);
				}
				break;
			case 10:
				if(GetCellItemOrientation(x, y, z) == 0 || GetCellItemOrientation(x, y, z) == 16){
					SetCellItem(x, y, z, CellItem.Crossing, 0);
				}
				break;
			case 22:
				if(GetCellItemOrientation(x, y, z) == 16 || GetCellItemOrientation(x, y, z) == 10){
					SetCellItem(x, y, z, CellItem.Crossing, 0);
				}
				break;
			case 16:
				if(GetCellItemOrientation(x, y, z) == 0 || GetCellItemOrientation(x, y, z) == 22){
					SetCellItem(x, y, z, CellItem.Crossing, 0);
				}
				break;
		}
	}

	private void SetCornerOnCorner(int x, int y, int z, int rot){
		switch (rot){
			case 0:
				switch(GetCellItemOrientation(x, y, z)){
					case 16:
						SetCellItem(x, y, z, CellItem.T, 16);
						break;
					case 22:
						SetCellItem(x, y, z, CellItem.T);
						break;
					case 10:
						SetCellItem(x, y, z, CellItem.Crossing);
						break;
				}
				break;
			case 10:
				switch(GetCellItemOrientation(x, y, z)){
					case 16:
						SetCellItem(x, y, z, CellItem.T, 10);
						break;
					case 22:
						SetCellItem(x, y, z, CellItem.T, 22);
						break;
					case 0:
						SetCellItem(x, y, z, CellItem.Crossing);
						break;
				}
				break;
			case 22:
				switch(GetCellItemOrientation(x, y, z)){
					case 10:
						SetCellItem(x, y, z, CellItem.T, 22);
						break;
					case 0:
						SetCellItem(x, y, z, CellItem.T, 16);
						break;
					case 16:
						SetCellItem(x, y, z, CellItem.Crossing);
						break;
				}
				break;
			case 16:
				switch(GetCellItemOrientation(x, y, z)){
					case 0:
						SetCellItem(x, y, z, CellItem.T, 16);
						break;
					case 10:
						SetCellItem(x, y, z, CellItem.T, 10);
						break;
					case 22:
						SetCellItem(x, y, z, CellItem.Crossing);
						break;
				}
				break;
		}
	}

	private void SetBridgeTile(){
		int direction = 0;
		int counting__ = 0;
		int arch_length = 4;
		List<Vector3[]> road_path_splitter9000= new List<Vector3[]>();
		Vector3[] brain_bridge_tiles = new Vector3[arch_length];
		bool bridge_shift = false;
		//if(zitter_gleich) road_path.Reverse();
		for(int i = 0; i < road_path.Count; i++){
			if(i%arch_length == 0){
				counting__ = 0;
				brain_bridge_tiles = new Vector3[arch_length];
				road_path_splitter9000.Add(brain_bridge_tiles);
			}
			if(i != road_path.Count-1){
				if(GetCellItem((int)road_path[i].x, (int)road_path[i].y, (int)road_path[i].z) == CellItem.Corner1 || GetCellItem((int)road_path[i].x, (int)road_path[i].y, (int)road_path[i].z) == CellItem.Corner2 && i%2 == 0){
					if(road_path[i].x < road_path[i+1].x){
						direction = 0; //east
					}
					else if(road_path[i].x > road_path[i+1].x){
						direction = 1; //west	
					}
					else if(road_path[i].z < road_path[i+1].z){
						direction = 2; //south
					}
					else if(road_path[i].z > road_path[i+1].z){
						direction = 3; //north
					}
				bridge_shift = false;
				}else{
					if(road_path[i].x < road_path[i+1].x){
						direction = 0; //east
					}
					else if(road_path[i].x > road_path[i+1].x){
						direction = 1; //west	
					}
					else if(road_path[i].z < road_path[i+1].z){
						direction = 2; //south
					}
					else if(road_path[i].z > road_path[i+1].z){
						direction = 3; //north
					}
				bridge_shift = true;
				}
				foreach(Vector3[] ELEMENT in road_path_splitter9000) GenerateBridgeArch(ELEMENT, bridge_shift, direction);
				first_arch = true;
				brain_bridge_tiles[counting__++] = road_path[i];
			}
		}
		road_path.Clear();
	}
	bool first_arch = true;
	int arch_pos = 0;
	private void GenerateBridgeArch(Vector3[] tiles, bool bridge_shift, int direction){
		int ypsilon_offset= -1;
		if(bridge_shift == true && first_arch){
			arch_pos = 1;
			first_arch = false;
		}
		for(;arch_pos < 4; arch_pos++){
			int x = (int)tiles.ElementAt(arch_pos).x;
			int y = (int)tiles.ElementAt(arch_pos).y;
			int z = (int)tiles.ElementAt(arch_pos).z;
			if(y == 0)return;
			y-=1;
				if(direction == 0){
					if(arch_pos == 0){
						for(;y >= 0; y--){
							SetBridgerinoItem(x, y, z, CellItem.Pillar);
						}
					}
					else if(arch_pos == 1){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 20);//mirrored y-plane
					}
					else if(arch_pos == 2){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Pillar);
					}
					else if(arch_pos == 3){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 18);//mirrored direction-plane
					}
				}else if(direction == 1){
					if(arch_pos == 0){
						for(;y >= 0; y--){
							SetBridgerinoItem(x, y, z, CellItem.Pillar);
						}
					}
					else if(arch_pos == 1){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 2);//mirrored y-plane
					}
					else if(arch_pos == 2){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Pillar);
					}
					else if(arch_pos == 3){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
					}
					
				}else if(direction == 2){
					if(arch_pos == 0){
						for(;y >= 0; y--){
							SetBridgerinoItem(x, y, z, CellItem.Pillar);
						}
					}
					else if(arch_pos == 1){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 18);//mirrored y-plane
					}
					else if(arch_pos == 2){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Pillar);
					}
					else if(arch_pos == 3){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 20);//mirrored direction-plane
					}
				}else if(direction == 3){
					if(arch_pos == 0){
						for(;y >= 0; y--){
							SetBridgerinoItem(x, y, z, CellItem.Pillar);
						}
					}
					else if(arch_pos == 1){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 2);//mirrored y-plane
					}
					else if(arch_pos == 2){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Pillar);
					}
					else if(arch_pos == 3){
						SetBridgerinoItem(x, (int)tiles.ElementAt(0).y+ypsilon_offset, z, CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
					}
				}
		}
		arch_pos = 0;
	}
	private void SetSlopeTile(int x, int y, int z, int rot){
		SetCellItem(x, y, z, CellItem.Slope, rot);
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

	private Vector3 GetNearestRoad(Vector3 pos){
		Vector3 nearestRoad = new Vector3();
		float closestDistance = width*length*height+1;
		foreach(Vector3 other in roadLocations){
			if(other != pos){
				if(Math.Abs(pos.z-(other).z)+Math.Abs(pos.x-(other).x) < closestDistance){
					closestDistance = Math.Abs(pos.z-(other).z)+Math.Abs(pos.x-(other).x);
					nearestRoad = other;	
				}
			}
		}
		if (GetCellItem((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z) == CellItem.Slope){
			Vector3 one = FollowRoadImproved((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, true, pos, Vector3.Zero);
			Vector3 two = FollowRoadImproved((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, false, pos, Vector3.Zero);
			return (pos.DistanceTo(one) < pos.DistanceTo(two)) ? one : two;
		}
		return nearestRoad;
	}
}
