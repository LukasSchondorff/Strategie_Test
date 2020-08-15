using Godot;
using System;

public class UnitBase {
    private Spatial unit_node;
    private Vector3 unit_position;
    private int unit_movementspeed;
    
    
    //--------------------------------------------------
    public UnitBase(Spatial unit_node, Vector3 cell_size) {
        this.unit_node = unit_node;
        setUnitPosition(this.unit_node.Translation / cell_size);
    }
    //--------------------------------------------------
    public void MoveUnit(Vector3 moveto, Vector3 cell_size) {
        this.unit_node.Translation = moveto * cell_size;
        this.setUnitPosition(moveto);
    }
    
    //--------------------------------------------------
    public void setUnitPosition(Vector3 unit_position) {
        this.unit_position = unit_position;
    }

    public Vector3 getUnitPosition() {
        return unit_position;
    }
}
