using Godot;
using System;

public class BuildingBase {

    private Vector3 building_location;
    private int hit_points;
    private int building_id;
    
    //--------------------------------------------------
    public void setBuildingID(int building_id) {
        this.building_id = building_id;
    }

    public void setHitPoints(int hit_points) {
        this.hit_points = hit_points;
    }
    
    public void setBuildingLocation(Vector3 building_location) {
        this.building_location = building_location;
    }

}
