using Godot;
using System;

public class UnitBase : Node  {
    
    
    // Called when the node enters the scene tree for the first time.
    private bool build = true;
    
    public override void _Ready() {
        
    }

    public void setbuild(bool build) {
        this.build = build;
    }
    
    public void switchbuild() {
        this.build = !this.build;
    } 
    
    public void mytest(Vector3 mouse_position, Vector3 cell_size) {
        //GD.Print(mouse_position + " !!!");
        mouse_position = new Vector3(mouse_position.x / cell_size.x, 
                                     mouse_position.y / cell_size.y,
                                     mouse_position.z / cell_size.z);
        GD.Print(mouse_position);
    }
    
    

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
