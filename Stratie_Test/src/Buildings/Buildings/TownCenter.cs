using Godot;
using System;

public class TownCenter : BuildingBase {
    
    public TownCenter(int hp, int id, Vector3 location) {
        setHitPoints(hp);
        setBuildingID(id);
        setBuildingLocation(location);
    }
}
