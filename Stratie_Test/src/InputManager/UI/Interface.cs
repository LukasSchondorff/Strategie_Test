using Godot;
using System;

public class Interface : Control {
    
    //-----------------------------------
    // Buildings
    private string pathtobuildings = "res://src/Buildings/Buildings/";

    private PlayerLevel playerlevel;

    public override void _Ready() {
        playerlevel = (PlayerLevel) GetNode("../PlayerLevel");
    }

    public void CallBuildingAction() {
        playerlevel.CurrentBuildingAction();
    }
}
