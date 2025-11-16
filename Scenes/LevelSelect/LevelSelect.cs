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
		var levelnum = GetNode<OptionButton>("%LevelNumber");
		int level = int.Parse(levelnum.GetItemText(levelnum.GetSelectedId()));
		manager.SetTargetLevelSave(level, 0);
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}
	private void _on_save_1_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		var levelnum = GetNode<OptionButton>("%LevelNumber");
		int level = int.Parse(levelnum.GetItemText(levelnum.GetSelectedId()));
		manager.SetTargetLevelSave(level, 1);
		CallDeferred(nameof(changescenes));
	}
	private void _on_save_2_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		var levelnum = GetNode<OptionButton>("%LevelNumber");
		int level = int.Parse(levelnum.GetItemText(levelnum.GetSelectedId()));
		GD.Print("Level: " + level);
		manager.SetTargetLevelSave(level, 2);
		CallDeferred(nameof(changescenes));
	}
	private void _on_new_level_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		var levelnum = GetNode<OptionButton>("%LevelNumber");
		int level = int.Parse(levelnum.GetItemText(levelnum.GetSelectedId()));
		GD.Print("Level: " + level);
		manager.SetTargetLevelSave(level, -1);
		CallDeferred(nameof(changescenes));

	}
	private void changescenes()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelUI/level_ui.tscn");
	}
	
	private void _on_mystery_level_pressed() {
		GD.Print("entering mystery level");
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		var levelnum = GetNode<OptionButton>("%LevelNumber");
		int level = int.Parse(levelnum.GetItemText(levelnum.GetSelectedId()));
		GD.Print("Level: " + level);
		manager.SetTargetLevelSave(level, -1);
		CallDeferred(nameof(changescenes));
	}
}
