using Godot;
using System;

public class BuildingGhosts : RoadGen
{
	private PackedScene outline_scene;
	private Spatial outline;
	private MeshInstance outline_mesh;

	private Material red;
	private Material green;

	[RemoteSync]
	public override void SetCellItem(int x, int y, int z, int itemIndex){
		((GridMap) GetNode("./")).SetCellItem(x,y,z,itemIndex);
	}
	public override void _Ready()
	{
		red = (Material)ResourceLoader.Load("res://Assets/Buildings/Outline/OutlineMaterialRed.tres");
		green = (Material)ResourceLoader.Load("res://Assets/Buildings/Outline/OutlineMaterialGreen.tres");

		outline_scene = (PackedScene)ResourceLoader.Load("res://Assets/Buildings/Outline/Outline.tscn");
		outline = (Spatial)outline_scene.Instance();
		outline.Name = "Outline";
		outline_mesh = (MeshInstance)outline.GetNode("luftballonmesherinorotzskelettbaumgraphtankstellenwartjareicht");
		AddChild(outline);

		base._Ready();
	}

	private const float rayLength = 1000;
	private Vector3 lastMouseSnap;
	public override void _Input(InputEvent @event){
		if (@event is InputEventMouseMotion eventMouseMotion){
			var camera = (Camera)GetNode("../CameraGimbal/InnerGimbal/Camera");
			var from = camera.ProjectRayOrigin(eventMouseMotion.Position);
			var to = from + camera.ProjectRayNormal(eventMouseMotion.Position) * rayLength;
			var spaceState = GetWorld().DirectSpaceState;
			var res = spaceState.IntersectRay(from, to, null, 0b10000000000000000000, true, true);
			
			if (res.Contains("position")){
				Vector3 mouse_pos = (Vector3) res["position"] / CellSize;
				Vector3 tranlated_mouse_pos = new Vector3((int)mouse_pos.x, (int)mouse_pos.y, (int)mouse_pos.z);

				if(tranlated_mouse_pos != lastMouseSnap){
					lastMouseSnap = tranlated_mouse_pos;

					outline.Translation = MapToWorld((int)lastMouseSnap.x, (int)lastMouseSnap.y, (int)lastMouseSnap.z);

					if(base.ValidBuildingLocation(lastMouseSnap)){
						outline_mesh.SetSurfaceMaterial(0, green);
						((MeshInstance)outline_mesh.GetNode("Colour")).Mesh.SurfaceSetMaterial(0, green);
					}
					else {
						outline_mesh.SetSurfaceMaterial(0, red);
						((MeshInstance)outline_mesh.GetNode("Colour")).Mesh.SurfaceSetMaterial(0, red);
					}
					GD.Print(lastMouseSnap);
				}
			}
		}
	}

}
