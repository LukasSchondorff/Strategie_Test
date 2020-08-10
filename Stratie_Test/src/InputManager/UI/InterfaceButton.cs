using Godot;
using System;

public class InterfaceButton : NinePatchRect {

    public void _on_Button_button_up() {
        ((Interface)GetNode("../../../../Interface")).CallBuildingAction();
    }
}
