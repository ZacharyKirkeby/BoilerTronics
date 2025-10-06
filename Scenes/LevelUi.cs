using Godot;
using System;

public partial class LevelUi : Node2D
{
	public override void _Ready() {
		var button = GetNode<Button>("MainVBox/PanelContainer/HBoxContainer/CategoryPicker/Movement");
		button.GrabFocus();
	}
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
	private void _on_movement_pressed() {
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Claw Items").Visible = false;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Factory Items").Visible = false;
		GetNode<ScrollContainer>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Movement Items").ScrollHorizontal = 0;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Movement Items").Visible = true;
	}
	private void _on_claw_pressed() {
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Factory Items").Visible = false;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Movement Items").Visible = false;
		GetNode<ScrollContainer>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Claw Items").ScrollHorizontal = 0;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Claw Items").Visible = true;
	}
	private void _on_factory_pressed() {
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Claw Items").Visible = false;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Movement Items").Visible = false;
		GetNode<ScrollContainer>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Factory Items").ScrollHorizontal = 0;
		GetNode<Control>("MainVBox/PanelContainer/HBoxContainer/PanelContainer/Factory Items").Visible = true;
	}
}
