using Godot;
using System;

public class UnitBase {
    private Node unit_node;
    private Vector3 unit_position;
    
    //--------------------------------------------------
    public UnitBase(Node unit_node, Vector3 cell_size) {
        this.unit_node = unit_node;
    }
    
    //--------------------------------------------------
    public void setUnitPosition(Vector3 unit_position) {
        this.unit_position = unit_position;
    }
    
}
