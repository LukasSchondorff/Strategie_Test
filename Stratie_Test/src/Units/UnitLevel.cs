using System;
using Godot;
using System.Collections.Generic;

public class UnitLevel {
    private BuildingLevel buildinglevel;
    private List<UnitBase> unit_base_list = new List<UnitBase>();
    private List<UnitBase> unit_selected_list = new List<UnitBase>();

    private Vector3 cellsize;
    private bool unit_selected = false;

    public enum Units {
        //Neutral Units
        Villager,
        //Aggrasive Units
        Warrior
    }

    public UnitLevel(Vector3 cellsize) {
        this.cellsize = cellsize;
    }
    //--------------------------------------------------
    public void ConfigureUnitAndAdd(Spatial unit_node, Units unit_type, Vector3 cellsize) {

        UnitBase unit;
        switch (unit_type) {
            case Units.Villager:
                //TODO transform to Villager when class is created
                unit = new UnitBase(unit_node, cellsize); 
                unit_base_list.Add(unit);
                break;
            case Units.Warrior:
                break;
            default:
                return;
        }
    }
    
    public bool CheckSpace(Vector3 pos1, Vector3 pos2) {
        //TODO ill make it only 2d and we will see how it works
        int x_1 = (int) pos1.x;
        int z_1 = (int) pos1.z;
        int x_2 = (int) pos2.x;
        int z_2 = (int) pos2.z;
        
        int swapper;
        if (x_2 < x_1) {
            swapper = x_1;
            x_1 = x_2;
            x_2 = swapper;
        } 
        if (z_2 < z_1) {
            swapper = z_1;
            z_1 = z_2;
            z_2 = swapper;
        }
        
        ClearSelectedUnits();
        foreach (var unit in unit_base_list) {
            Vector3 unit_pos = unit.getUnitPosition();

            if (x_1 < unit_pos.x && unit_pos.x < x_2) {
                if (z_1 < unit_pos.z && unit_pos.z < z_2) {
                    unit_selected_list.Add(unit);
                }
            }
        }

        if (unit_selected_list.Count == 0) {
            unit_selected = false;
            return true;
        } else {
            unit_selected = true;
            return false;
        }
    }

    public void MoveSelectedUnits(Vector3 moveto) {
        foreach (var unit in unit_selected_list) {
            unit.MoveUnit(moveto, cellsize);
        }
    }
    
    public void ClearSelectedUnits() {
        unit_selected_list.Clear();
    }
    
    //--------------------------------------------------
    public void setBuildingLevel(BuildingLevel buildinglevel) {
        this.buildinglevel = buildinglevel;
    }
    
    public bool selectedUnits() {
        return unit_selected;
    }
}
