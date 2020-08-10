using Godot;
using System;

public class Interface : Control {
    
    //-----------------------------------
    // Buildings
    private string pathtobuildings = "res://src/Buildings/Buildings/";

    private MapGen mapgen;

    public override void _Ready() {
        mapgen = (MapGen) GetNode("../GridMap");
    }

    public void CallBuildingAction() {
        mapgen.GetPlayerLevel().getBuildingLevel().CurrentBuildingAction();
    }
}
