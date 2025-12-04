using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using BoilerTronicsObjects.Placeable;	// use for TileTex

using Parsing;
using System.Linq;
public partial class LevelCreator : LevelUi
{
	string levelSavePath = ProjectSettings.GlobalizePath("user://LevelCreator/");

	public override void _Ready()
	{
		base._Ready();
		var fileLocation = GetNode<Label>("%FileLocation");
		fileLocation.Text = "Level will be saved at " + levelSavePath;

		string[] saveFiles = Directory.GetFiles(levelSavePath, "*.save");
		string[] userSaveFiles = Directory.GetFiles(ProjectSettings.GlobalizePath("res://Resources/Levels/"), "*.save");
		var dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");

		foreach (string saveFile in saveFiles)
		{
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		foreach (string saveFile in userSaveFiles) {
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		var saveWindow = GetNode<Window>("%CreatorLoadWindow");
		var loadName = GetNode<Label>("%LoadName");
		loadName.Text = dropdown.GetItemText(dropdown.Selected);
		saveWindow.Visible = true;
	}
	
	// opens level metadata edit button
	private void _on_edit_button_pressed()
	{
		var levelName = GetNode<LineEdit>("%MetaLevelName");
		var length = GetNode<LineEdit>("%MetaLength");
		var width = GetNode<LineEdit>("%MetaWidth");
		var backgroundTilesSelector = GetNode<OptionButton>("%BackgroundTilesSelector");
		
		// TODO: fill levelName, length, width, backgroundTilesSelector with default data
		// TODO: currently, length/width visuals are completely hidden because it'd take too much time to implement dynamically updating those visuals
		
		// pull current level name
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		levelName.SetText(manager.saveState.levelName);
		
		// pull current level dimensions
		Vector2I levelDimensions = manager.saveState.GetLevelDimensions();
		length.SetText(levelDimensions.X.ToString());
		width.SetText(levelDimensions.Y.ToString());
		
		// programatically fill up the background tile info
		backgroundTilesSelector.Clear();
		
		// TODO: current system just manually adds types, not very nice/programatically
		backgroundTilesSelector.AddItem("Background1", 0);
		backgroundTilesSelector.AddItem("DebugVisual", 1);
		
		GetNode<Window>("%MetadataWindow").Visible = true;
		// GetNode<Window>("%MetadataWindow").Visible = true;
	}
	private void _on_metadata_window_close_requested()
	{
		GetNode<Window>("%MetadataWindow").Visible = false;
	}
	private void _on_change_metadata_button_pressed()
	{
		// var metadataButton = GetNode<Button>("%ChangeMetadataButton");
		var levelName = GetNode<LineEdit>("%MetaLevelName");
		var length = GetNode<LineEdit>("%MetaLength");
		var width = GetNode<LineEdit>("%MetaWidth");
		var backgroundTilesSelector = GetNode<OptionButton>("%BackgroundTilesSelector");
		/* TODO: Ethen change stuff when pressed */
		
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// TODO: when/if dimension metadata is also editable, include checks for those fields too!
		if (levelName.GetText() != "") {
			
			// update save state text
			manager.saveState.levelName = levelName.GetText();
			
			switch (backgroundTilesSelector.GetSelectedId()) {
				case 0:
					// default dark floor tiles
					manager.saveState.boundaryTex = new TileTex(new Vector2I(0, 0), 6);
					break;
				case 1:
					// debug background that exists only to demonstrate that this functionality exists
					manager.saveState.boundaryTex = new TileTex(new Vector2I(0, 1), 6);
					break;
				default:
					GD.Print("LevelCreator.cs: Change Metadata Button: Could not find selected background object!");
					break;
			}
			
			// save data
			manager.SaveAutosave();
			
			// set system to load autosave
			manager.SetTargetLevelSave(-2, 0);
			CallDeferred(nameof(LoadedReloadScene));
		}
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
		
		var lengthNode = GetNode<LineEdit>("%NewLength");
		var widthNode = GetNode<LineEdit>("%NewWidth");
		
		/* TODO: Ethen implement new level logic using name from lineedit */
		
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		if (levelName.GetText() != ""
		&& levelID.GetText() != ""
		&& lengthNode.GetText() != ""
		&& widthNode.GetText() != "") {
			
			int levelIDNum = 0;
			int levelDimX = 5;
			int levelDimY = 5;
			try {
				levelIDNum = Int32.Parse(levelID.GetText());
				levelDimX = Int32.Parse(widthNode.GetText());
				levelDimY = Int32.Parse(lengthNode.GetText());
			} catch (FormatException e) {
				GD.Print("LevelCreator.cs: failed to create level, error: ", e.Message);
				return;
			}
			
			manager.saveState.levelName = levelName.GetText();
			manager.saveState.SetLevelID(levelIDNum);
			manager.saveState.SetLevelDimensions(new Vector2I(
				levelDimX,
				levelDimY
			));
			
			manager.creatingNewLevel = true;
			
			GetNode<Window>("%CreatorLoadWindow").Visible = false;
			GetNode<VBoxContainer>("%MainVBox").Visible = true;
			GetNode<CanvasLayer>("%ButtonTray").Visible = true;
			
			GD.Print("LevelCreator: creating new level: ", levelName.GetText());
		
			// fixes "_push_unhandled_input_internal: Condition "is_inside_tree()" is true" errors
			CallDeferred(nameof(LoadedReloadScene));
		}
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

	private void _on_export_and_upload_pressed()
    {
		string fileName = GetNode<LineEdit>("%NewFileName").GetText(); 
		/* TODO ADD UPLOAD TO SERVER */
        BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;
		GD.Print("LevelCreator: overwriting level: ", fileName);
		man.saveState.SaveDataTo(man, "LevelCreator", "/" + fileName);
		
		// close windows when done
		var saveWindow = GetNode<Window>("%CreatorSaveWindow");
		saveWindow.Visible = false;
    }

	private void _on_load_button_pressed()
    {
        var fileLocation = GetNode<Label>("%FileLocation");
		fileLocation.Text = "Level will be saved at " + levelSavePath;
		string[] saveFiles = Directory.GetFiles(levelSavePath, "*.save");
		var dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		dropdown.Clear();
		string[] userSaveFiles = Directory.GetFiles(ProjectSettings.GlobalizePath("res://Resources/Levels/"), "*.save");

		foreach (string saveFile in saveFiles)
		{
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		foreach (string saveFile in userSaveFiles) {
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		var saveWindow = GetNode<Window>("%CreatorLoadWindow");
		var loadName = GetNode<Label>("%LoadName");
		loadName.Text = dropdown.GetItemText(dropdown.Selected);
		saveWindow.Visible = true;
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
		
		manager.layerFloor.EditProtectedTiles(false);
		manager.layerFactory.EditProtectedTiles(false);
		manager.layerClaw.EditProtectedTiles(false);
		manager.layerRail.EditProtectedTiles(false);
		manager.layerMovement.EditProtectedTiles(false);
		
		
		switch (index) {
			case 0:
				// do nothing; this is the "none" option
				break;
			case 1:
				manager.layerFloor.HighlightProtectedTiles(true);
				manager.layerFloor.EditProtectedTiles(true);
				break;
			case 2:
				manager.layerFactory.HighlightProtectedTiles(true);
				manager.layerFactory.EditProtectedTiles(true);
				break;
			case 3:
				manager.layerClaw.HighlightProtectedTiles(true);
				manager.layerClaw.EditProtectedTiles(true);
				break;
			case 4:
				manager.layerRail.HighlightProtectedTiles(true);
				manager.layerRail.EditProtectedTiles(true);
				break;
			case 5:
				manager.layerMovement.HighlightProtectedTiles(true);
				manager.layerMovement.EditProtectedTiles(true);
				break;
			default:
				GD.Print("LevelCreator.cs: Protected Tiles Dropdown Menu: Invalid Index: ", index);
				break;
		}
	}
}
