using Godot;

public class TownCenter : BuildingBase {
    
    public TownCenter(int hp, int id, Vector3 location) {
        setBuildingName("Towncenter");
        setBuildingHitPoints(hp);
        setBuildingID(id);
        setBuildingLocation(location);
    }

    public Spatial ProduceVillager(Node unitlevel, Vector3 cellsize) {
        var unit_resource = (PackedScene)ResourceLoader.Load("res://Assets/Units/Unit.tscn");
        if (unit_resource != null) {
            var unit = (Spatial)unit_resource.Instance();
            unit.Translation = getBuildingLocation()*cellsize + Vector3.Up*1;
            unitlevel.AddChild(unit);
            return unit;
        }
        return null;
    }
    
}
