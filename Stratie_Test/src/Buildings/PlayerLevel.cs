using Godot;
using System;

public class PlayerLevel : GridMap {

    private Vector3 cell_size = new Vector3(3f,1.5f,3f);
    private BuildingBase[,] player_buildings;
    
    private Area arealevel;
    private Control gameinterface;
    
    private int offset_y = 1;

    public void init(Vector3 cell_size, int map_width, int map_lenght) {
        this.cell_size = cell_size;
        CellSize = cell_size;
        player_buildings = new BuildingBase[map_width,map_lenght];
        
        arealevel = (Area)GetNode("Area");
        arealevel.Connect("input_event", this, nameof(OnAreaInputEvent));

        gameinterface = (Control)GetNode("../Interface");
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
            UpdatePlayerBuildings(m_x, m_z, new TownCenter(100, 49,new Vector3(m_x, m_y, m_z)));
        } 
        setGameInterface(false);
    }

    private void UpdatePlayerBuildings(int x, int z, BuildingBase building_id) {
        player_buildings[x, z] = building_id;
    }

    private void GenerateCollisionArea(int m_x, int m_y, int m_z) {
        CollisionShape shape = new CollisionShape();
        shape.Shape = new BoxShape();
        shape.Translation = new Vector3(m_x*cell_size.x + cell_size.x/2, m_y*cell_size.y + offset_y, m_z*cell_size.z + cell_size.z/2);
        shape.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        arealevel.AddChild(shape);
        
        /*
        //Visual Representation
        MeshInstance mi = new MeshInstance();
        mi.Mesh = new CubeMesh();
        mi.Translation = new Vector3(m_x*cell_size.x + cell_size.x/2, m_y*cell_size.y + offset_y, m_z*cell_size.z + cell_size.z/2);
        mi.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        arealevel.AddChild(mi);
        */
    }

    public void OnAreaInputEvent(Camera camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx) {
        if (@event is InputEventMouseButton) {
            var mouse_event = (InputEventMouseButton) @event;
            if (mouse_event.ButtonIndex == (int) ButtonList.Left) {
                if (mouse_event.IsPressed()) {
                    setGameInterface(true);
                }
            }
        }
    }
    
    //--------------------------------------------------
    private void swapGameInterface() {
        gameinterface.Visible = !gameinterface.Visible; 
    }

    private void setGameInterface(bool visable) {
        gameinterface.Visible = visable; 
    }
}
