using Godot;
using System;

public class BuildingBase {

    private Vector3 building_location;
    private int building_hit_points;
    private int building_id;
    private String building_name;
    
    //--------------------------------------------------
    public void setBuildingID(int building_id) {
        this.building_id = building_id;
    }

    public int getBuildingID() {
        return building_id;
    }

    public void setBuildingHitPoints(int hit_points) {
        this.building_hit_points = hit_points;
    }
    
    public void setBuildingLocation(Vector3 building_location) {
        this.building_location = building_location;
    }

    public Vector3 getBuildingLocation() {
        return building_location;
    }

    public void setBuildingName(string building_name) {
        this.building_name = building_name;
    }

    public string getBuildingName() {
        return building_name;
    }

}
