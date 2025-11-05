using Godot;
using System;
using System.Collections.Generic;
using System.IO;

using Parsing;
public partial class LevelCreator : LevelUi
{
	string levelSavePath = ProjectSettings.GlobalizePath("user://LevelCreator/");

	public override void _Ready()
	{
		base._Ready();
		var fileLocation = GetNode<Label>("%FileLocation");
		fileLocation.Text = "Level will be saved at " + levelSavePath;
		string saveDir = ProjectSettings.GlobalizePath("res://Resources/Levels");
		string[] saveFiles = Directory.GetFiles(saveDir, "*.save");
		var dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		foreach (string saveFile in saveFiles)
		{
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		var saveWindow = GetNode<Window>("%CreatorLoadWindow");
		var loadName = GetNode<Label>("%LoadName");
		loadName.Text = dropdown.GetItemText(dropdown.Selected);
		saveWindow.Visible = true;
	}
	private void _on_creatorsave_button_pressed()
	{
		var saveWindow = GetNode<Window>("%CreatorSaveWindow");
		saveWindow.Visible = true;
	}

	private void _on_creator_save_window_close_requested()
	{
		var saveWindow = GetNode<Window>("%CreatorSaveWindow");
		saveWindow.Visible = false;
	}
	private void _on_creator_load_window_close_requested()
	{
		var loadWindow = GetNode<Window>("%CreatorLoadWindow");
		var loadName = GetNode<LineEdit>("%NewLevelName");
		loadName.Text = "";
		loadWindow.Visible = false;
		GetNode<VBoxContainer>("%MainVBox").Visible = true;
		GetNode<CanvasLayer>("%ButtonTray").Visible = true;
	}
	private void _on_new_level_button_pressed()
	{
		var levelName = GetNode<LineEdit>("%NewLevelName");
		var levelID = GetNode<LineEdit>("%NewLevelID");
		var newLevelButton = GetNode<Button>("%NewLevelButton");
		/* TODO: Ethen implement new level logic using name from lineedit */
		GetNode<Window>("%CreatorLoadWindow").Visible = false;
		GetNode<VBoxContainer>("%MainVBox").Visible = true;
		GetNode<CanvasLayer>("%ButtonTray").Visible = true;
	}
	private void _on_new_file_name_text_changed(String text)
	{
		var fileLocation = GetNode<Label>("%FileLocation");
		fileLocation.Text = "Level will be saved at " + levelSavePath + text + ".save";
	} 
	private void _on_existing_load_level_selector_item_selected(int index)
	{
		var loadName = GetNode<Label>("%LoadName");
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		loadName.Text = dropdown.GetItemText(index);
	}

	private void _on_export_button_pressed()
	{
		string fileName = GetNode<LineEdit>("%NewFileName").GetText();
		/* TODO: Ethen implement save logic 
			you can find the file name selected with 
			dropdown.GetItemText(dropdown.Selected);
			if you need full path youll probably have to store it in a variable 
			somewhere using logic later
			
			Ethen - done, thank you for the docs.
		*/
		
		BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;
		GD.Print("LevelCreator: overwriting level: ", fileName);
		man.saveState.SaveDataTo(man, "LevelCreator", "/" + fileName);
		
		// close windows when done
		var saveWindow = GetNode<Window>("%CreatorSaveWindow");
		saveWindow.Visible = false;
	}
	
	private void _on_load_level_button_pressed()
	{
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		/* TODO: Ethen implement load logic 
			you can find the file name selected with 
			dropdown.GetItemText(dropdown.Selected);
			if you need full path youll probably have to store it in a variable 
			somewhere using logic later
			
			Ethen - done, thank you for the docs.
		*/
		
		
		GetNode<Window>("%CreatorLoadWindow").Visible = false;
		GetNode<VBoxContainer>("%MainVBox").Visible = true;
		GetNode<CanvasLayer>("%ButtonTray").Visible = true;
		
		// updates manager field such that the level knows to load from a specific given level
		BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;
		man.loadLevelName = dropdown.GetItemText(dropdown.Selected);
		
		GD.Print("LevelCreator: loading level: ", man.loadLevelName);
		
		// fixes "_push_unhandled_input_internal: Condition "is_inside_tree()" is true" errors
		CallDeferred(nameof(LoadedReloadScene));
	}
	
	// reloads current scene
	private void LoadedReloadScene() {
		GetTree().ReloadCurrentScene();
	}

	private void _on_protected_tiles_pressed()
	{
		var protectedTilesWindow = GetNode<Window>("%ProtectedTilesWindow");
		protectedTilesWindow.Visible = true;
	}

	private void _on_protected_tiles_window_close_requested()
	{
		var protectedTilesWindow = GetNode<Window>("%ProtectedTilesWindow");
		protectedTilesWindow.Visible = false;
	}
	private void _on_protected_tiles_dropdown_item_selected(int index)
	{
		/* index values 
			0: none 
			1: Floor 
			2: Factory 
			3: Claw 
			4: Rail 
			5: Movement
			
			Ethen - thank you Ethan for the docs and etc
		*/
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.layerFloor.HighlightProtectedTiles(false);
		manager.layerFactory.HighlightProtectedTiles(false);
		manager.layerClaw.HighlightProtectedTiles(false);
		manager.layerRail.HighlightProtectedTiles(false);
		manager.layerMovement.HighlightProtectedTiles(false);
		
		switch (index) {
			case 0:
				// do nothing; this is the "none" option
				break;
			case 1:
				manager.layerFloor.HighlightProtectedTiles(true);
				break;
			case 2:
				manager.layerFactory.HighlightProtectedTiles(true);
				break;
			case 3:
				manager.layerClaw.HighlightProtectedTiles(true);
				break;
			case 4:
				manager.layerRail.HighlightProtectedTiles(true);
				break;
			case 5:
				manager.layerMovement.HighlightProtectedTiles(true);
				break;
			default:
				GD.Print("LevelCreator.cs: Protected Tiles Dropdown Menu: Invalid Index: ", index);
				break;
		}
	}
}
