using Godot;
using System;

public class BuildingLevel : GridMap{

    private Vector3 cell_size;
    private BuildingBase[,] player_buildings;
    private BuildingBase current_building;
    private UnitLevel unitlevel;

    private Area arealevel;
    private Control gameinterface;
    private GridMap playerlevel;

	private int offset_y = 1;
    private float epsilon = 0.05f; 

	public void init(Vector3 cell_size, int map_width, int map_lenght, GridMap playerlevel) {
		this.cell_size = cell_size;
        this.playerlevel = playerlevel;
        this.playerlevel.CellSize = cell_size;
		player_buildings = new BuildingBase[map_width,map_lenght];

        arealevel = (Area)playerlevel.GetNode("Area");
		arealevel.Connect("input_event", this, nameof(OnAreaInputEvent));

        gameinterface = (Control)playerlevel.GetNode("../Interface");
    }

	public void CheckSpace(Vector3 mouse_position) {
		mouse_position = new Vector3(mouse_position.x / cell_size.x, mouse_position.y / cell_size.y, mouse_position.z / cell_size.z);
		int m_x = (int) mouse_position.x;
		int m_y = (int) mouse_position.y;
		int m_z = (int) mouse_position.z;

		int clicked_cell = playerlevel.GetCellItem(m_x, m_y, m_z);
        if (clicked_cell == -1) { //if current cell is empty
            playerlevel.SetCellItem(m_x, m_y, m_z, 49);
            GenerateCollisionArea(m_x, m_y, m_z);
            UpdatePlayerBuildings(m_x, m_z, new TownCenter(100, 49,new Vector3(m_x, m_y, m_z)));
            //GD.Print(m_x + " " + m_z);
        }
        setGameInterface(false);
    }

    private void UpdatePlayerBuildings(int x, int z, BuildingBase building_id) {
        player_buildings[x, z] = building_id;
        current_building = building_id;
    }

    private void GenerateCollisionArea(int m_x, int m_y, int m_z) {
        CollisionShape shape = new CollisionShape();
        shape.Shape = new BoxShape();
        shape.Translation = new Vector3(m_x*cell_size.x + cell_size.x/2, m_y*cell_size.y + offset_y, m_z*cell_size.z + cell_size.z/2);
        shape.Scale = new Vector3(cell_size.x/2, 1, cell_size.z/2);
        arealevel.AddChild(shape);
    }

    public void OnAreaInputEvent(Camera camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx) {
        if (@event is InputEventMouseButton) {
            var mouse_event = (InputEventMouseButton) @event;
            if (mouse_event.ButtonIndex == (int) ButtonList.Right) {
                if (mouse_event.IsPressed()) {
                    click_position = new Vector3(click_position.x / cell_size.x, click_position.y / cell_size.y, click_position.z / cell_size.z);
                    click_position.x -= (int)System.Math.Round((click_normal.x),0,MidpointRounding.AwayFromZero) * epsilon;
                    click_position.z -= (int)System.Math.Round((click_normal.z),0,MidpointRounding.AwayFromZero) * epsilon;
                    
                    current_building = player_buildings[(int)click_position.x, (int)click_position.z];
                    setGameInterface(true);
                }
            } else if (mouse_event.ButtonIndex == (int) ButtonList.Left) {
                setGameInterface(false);
            }
        }
    }

    public void CurrentBuildingAction() {
        int id = current_building.getBuildingID();
        BuildingBase current_building_placeholder = current_building;
        switch (id) {
            case 49:
                ((TownCenter)current_building_placeholder).ProduceVillager(playerlevel.GetNode("Units"), cell_size);
                
                break;
            default:
                return;
        }
    }

    //--------------------------------------------------
    public void setUnitLevel(UnitLevel unitlevel) {
        this.unitlevel = unitlevel;
    }
    
    private void swapGameInterface() {
        gameinterface.Visible = !gameinterface.Visible;
    }

    private void setGameInterface(bool visable) {
        gameinterface.Visible = visable;
    }
}
