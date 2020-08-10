using Godot;
using System;

public class UnitLevel : Spatial {
    private BuildingLevel buildinglevel;

    public UnitLevel() {
        
    }
    //--------------------------------------------------
    public void setBuildingLevel(BuildingLevel buildinglevel) {
        this.buildinglevel = buildinglevel;
    }
}
