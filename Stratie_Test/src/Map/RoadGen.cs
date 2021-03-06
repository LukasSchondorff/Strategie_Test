using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class RoadGen : MapGen
{
	private bool zitter_gleich = false;
	public static class CellItem{
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

	private List<Vector3> buildingLocations;
	private List<Vector3> roadLocations;
	private BridgeGen bridgeGen;
	
	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int itemIndex){
		((GridMap) GetNode("./")).SetCellItem(x, y, z, itemIndex);
	}
	
	public override void _Ready()
	{
		buildingLocations = new List<Vector3>();
		roadLocations = new List<Vector3>();
		Connect("BuildingPlaced", this, nameof(AddBuilding));	
		bridgeGen = (BridgeGen)GetNode("BridgeGen");

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
			bool movedZ;
			Vector3 nearestRoad = GetNearestRoad(pos, out movedZ);

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
						GenerateRoad((movedZ)? nearestRoad : buildingLocations[buildingLocations.Count-1], (movedZ)? buildingLocations[buildingLocations.Count-1] : nearestRoad, roadTiles_with_rotation);
						zitter_gleich = true;
					}
					else {
						GenerateRoad((!movedZ)? buildingLocations[buildingLocations.Count-1] : nearestBuilding, (!movedZ)? nearestBuilding : buildingLocations[buildingLocations.Count-1], roadTiles_with_rotation);
						zitter_gleich = false;
					}
					return true;
				}
			}
			else{
				buildingLocations.Add(pos);
				if (nearestRoad != Vector3.Zero && pos.DistanceTo(nearestRoad) < pos.DistanceTo(nearestBuilding)){
					GenerateRoad((movedZ)? nearestRoad : buildingLocations[buildingLocations.Count-1], (movedZ)? buildingLocations[buildingLocations.Count-1] : nearestRoad, new List<int>());
					zitter_gleich = true;
				}
				else {
					GenerateRoad((!movedZ)? buildingLocations[buildingLocations.Count-1] : nearestBuilding, (!movedZ)? nearestBuilding : buildingLocations[buildingLocations.Count-1], new List<int>());
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
			bool movedZ;
			Vector3 nearestRoad = GetNearestRoad(pos, out movedZ);

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
		List<Vector3> road_path = new List<Vector3>();

		if(GetCellItem((int)pos1.x, (int)pos1.y, (int)pos1.z) != CellItem.Building && GetCellItem((int)pos1.x, (int)pos1.y, (int)pos1.z) != -1){
			if(GetCellItem((int)pos1.x, (int)pos1.y, (int)pos1.z) == CellItem.StraightRoad){
				if(pos1.x > pos2.x || pos1.z < pos2.z){
					SetCornerOnRoad((int)pos1.x, (int)pos1.y, (int)pos1.z, 10);
				}
				else{
					SetCornerOnRoad((int)pos1.x, (int)pos1.y, (int)pos1.z, 0);
				}
			}
			else{
				if(pos1.x != pos2.x){
					SetRoadTile((int)pos1.x, (int)pos1.y, (int)pos1.z, 0);
				}
				else {
					SetRoadTile((int)pos1.x, (int)pos1.y, (int)pos1.z, 16);
				}
			}
		}
		if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z) != CellItem.Building && GetCellItem((int)pos1.x, (int)pos1.y, (int)pos1.z) != -1){
			if(GetCellItem((int)pos2.x, (int)pos2.y, (int)pos2.z) == CellItem.StraightRoad){
				if(pos2.x > pos1.x || pos2.z < pos1.z){
					SetCornerOnRoad((int)pos2.x, (int)pos2.y, (int)pos2.z, 0);
				}
				else{
					SetCornerOnRoad((int)pos2.x, (int)pos2.y, (int)pos2.z, 10);
				}
			}
			else{
				if(pos2.x != pos1.x){
					SetRoadTile((int)pos2.x, (int)pos2.y, (int)pos2.z, 0);
				}
				else {
					SetRoadTile((int)pos2.x, (int)pos2.y, (int)pos2.z, 16);
				}
			}
		}

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
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z));
				}
				else{
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y + ((up)? y : y-1), (int)pos1.z));
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
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z));
				}
				else{
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y + ((up)? y : y-1), (int)pos1.z));
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z, slope_xOrientation);
				}
			}
		}
		RandomNumberGenerator rand = new RandomNumberGenerator();
		rand.Randomize();


		if(pos1.x != pos2.x && pos1.z != pos2.z){
			roadIndex++;
			road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z));
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
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z));
				}
				else{
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y + ((up)? y : y-1), (int)pos1.z+z));
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}
		else {
			for (z = -1; z > -Math.Abs((int)pos1.z - (int)pos2.z); z--){
				if(!additations.Contains(roadIndex++)){
					SetRoadTile((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z, CellItem.Corner1);
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y+y, (int)pos1.z+z));
				}
				else{
					road_path.Add(new Vector3((int)pos1.x+x, (int)pos1.y + ((up)? y : y-1), (int)pos1.z+z));
					SetSlopeTile((int)pos1.x+x, (int)pos1.y + ((up)? y++ : y---1), (int)pos1.z+z, slope_zOrientation);
				}
			}
		}
		bridgeGen.SetBridgeTile(road_path);
	}

	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int item, int rot){
		
		if(GetCellItem(x,y,z) != CellItem.Crossing){
			((GridMap) GetNode("./")).SetCellItem(x, y, z, item, rot);
			roadLocations.Add(new Vector3(x, y, z));
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

	private Vector3 FollowRoadImprovedX(int x, int y, int z, bool himmelsrichtung, Vector3 orgiana, Vector3 lastStraightRoad){
		bool reached_x = false;
		if (himmelsrichtung){
			reached_x = x <= (int)orgiana.x;
		}
		else {
			reached_x = x >= (int)orgiana.x;
		}
		switch (GetCellItem(x, y, z)){
			case CellItem.Crossing:
			case CellItem.StraightRoad:
				if (reached_x){
					return new Vector3(x, y, z);
				}
				Vector3 currRoad = new Vector3(x, y, z);
				x += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedX(x, y, z, himmelsrichtung, orgiana, currRoad);
			case CellItem.Corner1:
			case CellItem.Corner2:
				return new Vector3(x, y, z);
			case CellItem.Slope:
				y++;
				x += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedX(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
			case -1:
				y--;
				if (GetCellItem(x, y, z) == CellItem.Building){
					x += (himmelsrichtung)? -1 : 1;
					if (FollowRoadImprovedX(x, y, z, himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
						return lastStraightRoad;
					}
					else {
						return FollowRoadImprovedX(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
					}
				}
				else{
					return Vector3.Zero;
				}
			case CellItem.Building:
				return lastStraightRoad;
			case CellItem.T:
				if (FollowRoadImprovedX(x, y, z + ((himmelsrichtung)? -1 : 1), himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
					return new Vector3(x, y, z);
				}
				if (reached_x){
					return new Vector3(x, y, z);
				}
				x += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedX(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
		}

		return Vector3.Zero;
	}

	private Vector3 FollowRoadImprovedZ(int x, int y, int z, bool himmelsrichtung, Vector3 orgiana, Vector3 lastStraightRoad){
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
					return new Vector3(x, y, z);
				}
				Vector3 currRoad = new Vector3(x, y, z);
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedZ(x, y, z, himmelsrichtung, orgiana, currRoad);
			case CellItem.Corner1:
			case CellItem.Corner2:
				return new Vector3(x, y, z);
			case CellItem.Slope:
				y++;
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedZ(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
			case -1:
				y--;
				if (GetCellItem(x, y, z) == CellItem.Building){
					z += (himmelsrichtung)? -1 : 1;
					if (FollowRoadImprovedZ(x, y, z, himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
						return lastStraightRoad;
					}
					else {
						return FollowRoadImprovedZ(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
					}
				}
				else{
					return Vector3.Zero;
				}
			case CellItem.Building:
				return lastStraightRoad;
			case CellItem.T:
				if (FollowRoadImprovedZ(x, y, z + ((himmelsrichtung)? -1 : 1), himmelsrichtung, orgiana, lastStraightRoad) == Vector3.Zero){
					return new Vector3(x, y, z);
				}
				if (reached_z){
					return new Vector3(x, y, z);
				}
				z += (himmelsrichtung)? -1 : 1;
				return FollowRoadImprovedZ(x, y, z, himmelsrichtung, orgiana, lastStraightRoad);
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

	private Vector3 GetNearestRoad(Vector3 pos, out bool movedZ){
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
			Vector3 one = FollowRoadImprovedX((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, true, pos, Vector3.Zero);
			Vector3 two = FollowRoadImprovedX((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, false, pos, Vector3.Zero);
			Vector3 three = FollowRoadImprovedZ((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, false, pos, Vector3.Zero);
			Vector3 four = FollowRoadImprovedZ((int)nearestRoad.x, (int)nearestRoad.y, (int)nearestRoad.z, true, pos, Vector3.Zero);
			
			Vector3 five = (pos.DistanceTo(three) < pos.DistanceTo(four)) ? three : four; 
			Vector3 six = (pos.DistanceTo(one) < pos.DistanceTo(two)) ? one : two;
			movedZ = (pos.DistanceTo(five) < pos.DistanceTo(six));
			return (pos.DistanceTo(five) < pos.DistanceTo(six)) ? five : six;
		}
		movedZ = true;
		return nearestRoad;
	}
}
