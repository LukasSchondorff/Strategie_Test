using Godot;
using System;

// Only change scale of unit in the spatial node!!!
public class Fighter : Spatial
{
	private Area area_of_effect;
	private Area hitbox_area;
	
	private CollisionShape aoe_dmg_collison;
	private CollisionShape hitbox_collision;

	private Vector3 aoe_scale = new Vector3(2,0,2);

	private float area_of_effect_offset = 1.5f;

	private Vector2 hitbox_scale = new Vector2(1,12);

	public override void _Ready()
	{  
		// That's how animations work
		((AnimationPlayer)GetNode("AnimationPlayer")).PlaybackActive = true;
		((AnimationPlayer)GetNode("AnimationPlayer")).Autoplay = "Attack";
		((AnimationPlayer)GetNode("AnimationPlayer")).Play("Attack");
		
		aoe_scale *= ((MeshInstance)GetNode("HumanArmature/Knight")).Scale;
		
		// Create area for DMG query of other units
		area_of_effect = new Area();
		aoe_dmg_collison = new CollisionShape();
		BoxShape shape = new BoxShape();
		shape.Extents = aoe_scale;
		aoe_dmg_collison.Shape = shape;

		area_of_effect.Translation += new Vector3(0, 0, aoe_scale.z * area_of_effect_offset );
		area_of_effect.Name = "DMG_Area";
		area_of_effect.AddChild(aoe_dmg_collison);
		AddChild(area_of_effect);        

		// Create area for collsion with other units
		hitbox_area = new Area();
		hitbox_collision = new CollisionShape();
		CylinderShape hitbox_shape = new CylinderShape();
		hitbox_shape.Height = hitbox_scale.y;
		hitbox_shape.Radius = hitbox_scale.x;
		hitbox_collision.Shape = hitbox_shape;

		hitbox_area.Name = "Hitbox_Area";
		hitbox_area.AddChild(hitbox_collision);
		AddChild(hitbox_area);
	}
}
