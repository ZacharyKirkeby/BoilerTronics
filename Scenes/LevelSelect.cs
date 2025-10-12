using Godot;
using System;

public partial class LevelSelect : Node2D
{
	private void _on_play_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/level_ui.tscn");
	}
	private void _on_back_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
