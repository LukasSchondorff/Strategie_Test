using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class BridgeGen : Node
{
	RoadGen roadGen;
	bool first_arch = true;
	int i = 0;
	int arch_length = 4;
	public override void _Ready()
	{
		roadGen = (RoadGen)GetParent();
	}
	
	public void SetBridgeTile(List<Vector3> road_path){
		if(road_path.Count == 0) return;
		
		bool bridge_shift = false;
		bool reversed = false;
		if (road_path[road_path.Count-1].y <= road_path[0].y){
			road_path.Reverse();
			reversed = true;
			GD.Print("Reversed");
			if(Math.Abs((road_path[road_path.Count-1].z - road_path[0].z)) % 2 != 0){
				bridge_shift = true;
			}
		}
		else{
			if(Math.Abs((road_path[road_path.Count-1].x - road_path[0].x)) % 2 == 0){
				bridge_shift = true;
			}
		}
		GD.Print(bridge_shift);

		int direction = 4;
		int direction1 = 4;
		i = 0;

		first_arch = true;
		Vector3 corner = new Vector3(-100,-100,-100);
		foreach(Vector3 elem in road_path){
			if(roadGen.GetCellItem((int)elem.x, (int)elem.y, (int)elem.z) == RoadGen.CellItem.Corner1 || roadGen.GetCellItem((int)elem.x, (int)elem.y, (int)elem.z) == RoadGen.CellItem.Corner2){
				corner = elem;
				if(road_path[0].x < elem.x){
					direction = 0; //east
				}
				else if(road_path[0].x > elem.x){
					direction = 1; //west	
				}
				else if(road_path[0].z < elem.z){
					direction = 2; //south
				}
				else if(road_path[0].z > elem.z){
					direction = 3; //north
				}

				if(road_path[road_path.Count-1].x > elem.x){
					direction1 = 0; //east
				}
				else if(road_path[road_path.Count-1].x < elem.x){
					direction1 = 1; //west	
				}
				else if(road_path[road_path.Count-1].z > elem.z){
					direction1 = 2; //south
				}
				else if(road_path[road_path.Count-1].z < elem.z){
					direction1 = 3; //north
				}
			}
		}
		if(direction == 4 || direction1 == 4){
			Vector3 elem = road_path[road_path.Count-1];
			if(road_path[0].x < elem.x){
				direction = 0; //east
			}
			else if(road_path[0].x > elem.x){
				direction = 1; //west	
			}
			else if(road_path[0].z < elem.z){
				direction = 2; //south
			}
			else if(road_path[0].z > elem.z){
				direction = 3; //north
			}
			direction1 = direction;
		}	
		if(corner.x == -100 && direction == 4) return; 

		GD.Print(direction, " | ", direction1);

		for(int q = i; q < road_path.Count;){
			if(q >= road_path.IndexOf(corner)){
				GenerateBridgeArch(road_path, road_path[q], bridge_shift, reversed, direction1, direction);
				q = i;
				/*
				if (!GenerateBridgeArch(road_path, road_path[q], bridge_shift, reversed, direction1)){
					q = i;
				}
				else{	
					q += arch_length;
				}
				*/
			}
			else{
				GenerateBridgeArch(road_path, road_path[q], bridge_shift, reversed, direction, direction1);
				q = i;
				/*
				if (!GenerateBridgeArch(road_path, road_path[q], bridge_shift, reversed, direction)){
					q = i;
				}
				else{
					q += arch_length;
				}
				*/
			}
		}
		road_path.Clear();
	}

	private bool GenerateBridgeArch(List<Vector3> road_path, Vector3 first, bool bridge_shift, bool reversed, int direction, int direction1){
		int ypsilon_offset = -1;

		if(i >= road_path.Count) return false;
		int x, y, z;

		if(bridge_shift == true && first_arch){
			first_arch = false;
		}
		else{
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			if(roadGen.GetCellItem(x, y - ypsilon_offset, z) == RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, y - ypsilon_offset, z) == RoadGen.CellItem.Corner2){
				direction = direction1;
			}

			for(;y >= 0; y--){
				roadGen.SetBridgerinoItem(x, y, z, RoadGen.CellItem.Pillar);
			}
			if(roadGen.GetCellItem((int)first.x, (int)first.y, (int)first.z) == RoadGen.CellItem.Slope){
				first.y++;
			}
		}
		
		if(i >= road_path.Count) return false;
		x = (int)road_path[i].x;
		y = (int)road_path.ElementAt(i).y + ypsilon_offset;
		z = (int)road_path.ElementAt(i).z;
		i++;
		if(y < 0)return false;

		if(direction == 0){
			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 8);//mirrored y-plane
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Pillar);
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			if(first.x != x && first.z != z){
				if(direction1 == 1){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
				}
				else if(direction1 == 2){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 18);//mirrored direction-plane
				}
				else{
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 20);//mirrored direction-plane
				}
			}
			else{
				roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 2);//mirrored direction-plane
			}
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}
		}else if(direction == 1){
			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 2);//mirrored y-plane
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Pillar);
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			if(first.x != x && first.z != z){
				if(direction1 == 0){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 2);//mirrored direction-plane
				}
				else if(direction1 == 2){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 18);//mirrored direction-plane
				}
				else{
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 20);//mirrored direction-plane
				}
			}
			else{
				roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
			}
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}
		}else if(direction == 2){
			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 20);//mirrored y-plane
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Pillar);
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;
			
			if(first.x != x && first.z != z){
				if(direction1 == 0){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 2);//mirrored direction-plane
				}
				else if(direction1 == 1){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
				}
				else{
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 20);//mirrored direction-plane
				}
			}
			else{
				roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 18);//mirrored direction-plane
			}
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}
		}else if(direction == 3){
			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 18);//mirrored y-plane
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Pillar);
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}

			if(i >= road_path.Count) return false;
			x = (int)road_path[i].x;
			y = (int)road_path.ElementAt(i).y + ypsilon_offset;
			z = (int)road_path.ElementAt(i).z;
			i++;
			if(y < 0)return false;

			if(first.x != x && first.z != z){
				if(direction1 == 0){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 2);//mirrored direction-plane
				}
				else if(direction1 == 1){
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 8);//mirrored direction-plane
				}
				else{
					roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 18);//mirrored direction-plane
				}
			}
			else{
				roadGen.SetBridgerinoItem(x, (int)first.y+ypsilon_offset, z, RoadGen.CellItem.Slope_Arch_OverKopf, 20);//mirrored direction-plane
			}
			for(int b = 0; roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner1 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Corner2 && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Crossing && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.Slope && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.T && roadGen.GetCellItem(x, (int)first.y+b, z) != RoadGen.CellItem.StraightRoad; b++){
				roadGen.SetBridgerinoItem(x, (int)first.y+b, z, RoadGen.CellItem.Pillar);
			}
		}

		return true;
	}
}
