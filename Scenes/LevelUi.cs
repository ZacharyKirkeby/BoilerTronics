using Godot;
using System;

public partial class LevelUi : Node2D
{
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
