using Godot;
using System;

public class PlayerLevel : GridMap {

    private Vector3 cell_size = new Vector3(3f,1.5f,3f);
    private BuildingBase[,] player_buildings;
    
    private Area arealevel;
    private int offset_y = 1;

    public void init(Vector3 cell_size, int map_width, int map_lenght) {
        this.cell_size = cell_size;
        CellSize = cell_size;
        arealevel = (Area)GetNode("Area");
        arealevel.Connect("input_event", this, nameof(OnAreaInputEvent));
        player_buildings = new BuildingBase[map_width,map_lenght];
        
    }

    public void CheckSpace(Vector3 mouse_position) {
        mouse_position = new Vector3(mouse_position.x / cell_size.x, mouse_position.y / cell_size.y, mouse_position.z / cell_size.z);
        int m_x = (int) mouse_position.x;
        int m_y = (int) mouse_position.y;
        int m_z = (int) mouse_position.z;

        int clicked_cell = GetCellItem(m_x, m_y, m_z);

        
        if (clicked_cell == -1) { //if current cell is empty
            SetCellItem(m_x, m_y, m_z, 49);
            GenerateCollisionArea(m_x, m_y, m_z);
            GD.Print(m_x + " " + m_y + " " + m_z);
        }
    }

    private void UpdatePlayerBuildings(int x, int y, BuildingBase building_id) {
        player_buildings[x, y] = building_id;
    }

    private void GenerateCollisionArea(int m_x, int m_y, int m_z) {
        CollisionShape shape = new CollisionShape();
        shape.Shape = new BoxShape();
        shape.Translation = new Vector3(m_x*cell_size.x, m_y*cell_size.y + offset_y, m_z*cell_size.z);
        shape.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        arealevel.AddChild(shape);
        
        
        //Visual Representation
        MeshInstance mi = new MeshInstance();
        mi.Mesh = new CubeMesh();
        mi.Translation = new Vector3(m_x*cell_size.x, m_y*cell_size.y + offset_y, m_z*cell_size.z);
        mi.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        arealevel.AddChild(mi);
        
        //GD.Print(shape.Transform.origin);
    }

    public void OnAreaInputEvent(Camera camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx) {
        if (@event is InputEventMouseButton) {
            GD.Print("Test");
        }
    }
    
}
