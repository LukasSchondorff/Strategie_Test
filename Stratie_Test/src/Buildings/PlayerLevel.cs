using Godot;
using System;

public class PlayerLevel : GridMap {

    private Vector3 cell_size = new Vector3(3f,1.5f,3f);
    private Area arealevel;
    private int offset_y = 1;

    enum action {
        Build,
        Walk
    }
    
    public override void _Ready() {
        
    }

    public void init(Vector3 cell_size) {
        this.cell_size = cell_size;
        CellSize = cell_size;
        arealevel = (Area)GetNode("Area");
    }

    public void CheckSpace(Vector3 mouse_position) {
        mouse_position = new Vector3(mouse_position.x / cell_size.x, mouse_position.y / cell_size.y, mouse_position.z / cell_size.z);
        int m_x = (int) mouse_position.x;
        int m_y = (int) mouse_position.y;
        int m_z = (int) mouse_position.z;

        int clicked_cell = GetCellItem(m_x, m_y, m_z+1);

        if (clicked_cell == -1) { //if current cell is empty
            SetCellItem(m_x, m_y, m_z+1, 74);
            //GenerateCollisionArea(m_x, m_y, m_z);
        }
    }

    private void GenerateCollisionArea(int m_x, int m_y, int m_z) {
        BoxShape box = new BoxShape();

        CollisionShape shape = new CollisionShape();
        shape.Shape = box;
        shape.Translation = new Vector3(m_x*cell_size.x + cell_size.x/2, m_y*cell_size.y + offset_y, m_z*cell_size.z + cell_size.z/2);
        shape.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        
        
        this.AddChild(shape);
        GD.Print(shape.Transform.origin);
    }
    
}
