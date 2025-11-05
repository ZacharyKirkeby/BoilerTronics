using Godot;
using System;
using System.Collections.Generic;
using System.IO;

using Parsing;
public partial class LevelCreator : LevelUi
{

	public override void _Ready()
	{
		base._Ready();
		string saveDir = ProjectSettings.GlobalizePath("res://Resources/Levels");
		string[] saveFiles = Directory.GetFiles(saveDir, "*.save");
		var dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		foreach (string saveFile in saveFiles)
		{
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		var saveWindow = GetNode<Window>("%CreatorLoadWindow");
		var saveName = GetNode<Label>("%SaveName");
		saveName.Text = dropdown.GetItemText(dropdown.Selected);
		saveWindow.Visible = true;
	}
	private void _on_creatorsave_button_pressed()
	{
		string saveDir = ProjectSettings.GlobalizePath("res://Resources/Levels");
		string[] saveFiles = Directory.GetFiles(saveDir, "*.save");
		var dropdown = GetNode<OptionButton>("%ExistingLevelSelector");
		foreach (string saveFile in saveFiles)
		{
			dropdown.AddItem(Path.GetFileNameWithoutExtension(saveFile));
		}
		var saveWindow = GetNode<Window>("%CreatorSaveWindow");
		var saveName = GetNode<Label>("%SaveName");
		saveName.Text = dropdown.GetItemText(dropdown.Selected);
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
		var fileName = GetNode<LineEdit>("%NewFileName");
		var levelID = GetNode<LineEdit>("%NewLevelID");
		var newLevelButton = GetNode<Button>("%NewLevelButton");
		/* TODO: Ethen implement new level logic using name from lineedit */
		GetNode<Window>("%CreatorLoadWindow").Visible = false;
		GetNode<VBoxContainer>("%MainVBox").Visible = true;
		GetNode<CanvasLayer>("%ButtonTray").Visible = true;
	}
	private void _on_existing_level_selector_item_selected(int index)
	{
		var saveName = GetNode<Label>("%SaveName");
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLevelSelector");
		saveName.Text = dropdown.GetItemText(index);
	}
	private void _on_existing_load_level_selector_item_selected(int index)
	{
		var loadName = GetNode<Label>("%LoadName");
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		loadName.Text = dropdown.GetItemText(index);
	}

	private void _on_overwrite_save_button_pressed()
	{
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLevelSelector");
		/* TODO: Ethen implement save logic 
			you can find the file name selected with 
			dropdown.GetItemText(dropdown.Selected);
			if you need full path youll probably have to store it in a variable 
			somewhere using logic later
		*/
	}
	private void _on_load_level_button_pressed()
	{
		OptionButton dropdown = GetNode<OptionButton>("%ExistingLoadLevelSelector");
		/* TODO: Ethen implement load logic 
			you can find the file name selected with 
			dropdown.GetItemText(dropdown.Selected);
			if you need full path youll probably have to store it in a variable 
			somewhere using logic later
		*/
		GetNode<Window>("%CreatorLoadWindow").Visible = false;
		GetNode<VBoxContainer>("%MainVBox").Visible = true;
		GetNode<CanvasLayer>("%ButtonTray").Visible = true;
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
		/* index values 1: none 2: Floor 3: Factory 4: Claw 5: Rail 6: Movement 
			TODO: ethen link in protected tile highlighting based on which item is selected / clear if none*/
	}
}
