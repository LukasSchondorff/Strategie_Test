using System;
using Godot;
using System.Collections.Generic;

public class UnitLevel {
    private BuildingLevel buildinglevel;
    private List<UnitBase> unit_base_list = new List<UnitBase>();

    public enum Units {
        //Neutral Units
        Villager,
        //Aggrasive Units
        Warrior
    }

    public UnitLevel() {
        
    }
    //--------------------------------------------------
    public void ConfigureUnitAndAdd(Node unit_node, Units unit_type, Vector3 cellsize) {

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
    
    public void CheckSpace(Vector3 pos1, Vector3 pos2) {
        //TODO ill make it only 2d and we will see how it works
        foreach (var unit in unit_base_list) {
        }
    }
    
    //--------------------------------------------------
    public void setBuildingLevel(BuildingLevel buildinglevel) {
        this.buildinglevel = buildinglevel;
    }
}
