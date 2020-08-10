using Godot;
using System;

public class TownCenter : BuildingBase {
    
    public TownCenter(int hp, int id, Vector3 location) {
        setBuildingName("Towncenter");
        setBuildingHitPoints(hp);
        setBuildingID(id);
        setBuildingLocation(location);
    }

    public void ProduceVillager(Node unitlevel, Vector3 cellsize) {
        MeshInstance mi = new MeshInstance();
        mi.Mesh = new SphereMesh();
        mi.Translation = getBuildingLocation()*cellsize;
        unitlevel.AddChild(mi);
    }
    
}
