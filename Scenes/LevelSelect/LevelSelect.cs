using Godot;
using System;



public partial class LevelSelect : Node2D
{
	private void _on_play_pressed() {
		GetNode<Window>("Window").Visible = true;
	}
	private void _on_back_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu/main_menu.tscn");
	}
	private void _on_window_close_requested() {
		GetNode<Window>("Window").Visible = false;
	}
	private void _on_save_0_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 0);
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}
	private void _on_save_1_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 1);
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}
	private void _on_save_2_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 2);
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}
}
