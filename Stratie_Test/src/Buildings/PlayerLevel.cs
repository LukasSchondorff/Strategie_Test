using Godot;
using System;

public class PlayerLevel {

    //--------------------------------------------------
    private BuildingLevel buildinglevel;
    private UnitLevel unitlevel;
    
    private Vector3 cell_size = new Vector3(3f,1.5f,3f);
    
    //--------------------------------------------------
    public PlayerLevel(Vector3 cell_size, int map_width, int map_lenght, GridMap playerlevel) {
        buildinglevel = new BuildingLevel();
        buildinglevel.init(cell_size, map_width, map_lenght, playerlevel);
        unitlevel = new UnitLevel();
        
        setConnectionsBetweenLevels();
    }

    private void setConnectionsBetweenLevels() {
        buildinglevel.setUnitLevel(unitlevel);
        unitlevel.setBuildingLevel(buildinglevel);
    }
    //--------------------------------------------------
    public void CheckSpaceAndDecide(Vector3 pos1, Vector3 pos2) {
        //TODO add prio so that we need to only check one of them
        buildinglevel.CheckSpace(pos1, pos2);
        //unitlevel.CheckSpace(pos1, pos2);
    }
    //--------------------------------------------------
    public BuildingLevel getBuildingLevel() {
        return buildinglevel;
    }
    
    public UnitLevel getUnitLevel() {
        return unitlevel;
    }
}
