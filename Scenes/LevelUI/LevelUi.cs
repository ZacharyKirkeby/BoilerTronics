using Godot;
using System;
using System.Collections.Generic;

using Parsing;
public partial class LevelUi : Node2D
{
	/* Editors */

	private int currentLine = 0;
	private TabContainer tabs;
	private Parser parser;

	/* Steps */

	private Label stepCountLabel;
	private int stepCount = 0;

	/* Save Box ? (Ethan Change name for clarification) */

	private StyleBoxFlat sbf = new StyleBoxFlat();
	private StyleBoxFlat sbe = new StyleBoxFlat();
	private StyleBoxFlat sbeh = new StyleBoxFlat();
	private StyleBoxFlat sbfh = new StyleBoxFlat();

	/* Buttons */

	private Button saveZero;
	private Button saveOne;
	private Button saveTwo;
	private Button clearZero;
	private Button clearOne;
	private Button clearTwo;
	private Button stepButton;

	private bool stepDisabled = false;

	public override void _Ready()
	{
		tabs = GetNode<TabContainer>("/root/Node2D/MainVBox/TerminalLevelSplit/TerminalContainer");


		saveZero = GetNode<Button>("Window/SaveContainer/Save0Cont/Save 0");
		saveOne = GetNode<Button>("Window/SaveContainer/Save1Cont/Save 1");
		saveTwo = GetNode<Button>("Window/SaveContainer/Save2Cont/Save 2");
		clearZero = GetNode<Button>("Window/SaveContainer/Save0Cont/Clear 0");
		clearOne = GetNode<Button>("Window/SaveContainer/Save1Cont/Clear 1");
		clearTwo = GetNode<Button>("Window/SaveContainer/Save2Cont/Clear 2");
		sbf.BgColor = new Color(1, 0, 0);
		sbf.BorderColor = new Color(0, 0, 0);
		sbf.SetBorderWidthAll(3);
		sbf.SetCornerRadiusAll(20);
		sbfh = sbf.Duplicate() as StyleBoxFlat;
		sbfh.BorderColor = new Color(1, 1, 1);
		sbe.BgColor = new Color(0, 0.7f, 0);
		sbe.BorderColor = new Color(0, 0, 0);
		sbe.SetBorderWidthAll(3);
		sbe.SetCornerRadiusAll(20);
		sbeh = sbe.Duplicate() as StyleBoxFlat;
		sbeh.BorderColor = new Color(1, 1, 1);
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		stepButton = GetNode<Button>("%Step Button");
		UpdateStepCount();
		// manager.SetDraggable(false); // debug; testing script
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.E = new ErrorHandler();
		AddChild(manager.currLevel.E); // Add as child so that we can access elements in the level

		parser = GetNode<Parser>("/root/Node2D/MainVBox/TerminalLevelSplit/Parser");
		parser.Connect(Parser.SignalName.ErrorRaised, new Callable(this, nameof(manager.currLevel.E.OnParserErrorRaised)));

		manager.currLevel.P = parser;

		var button = GetNode<Button>("MainVBox/PanelContainer/HBoxContainer/CategoryPicker/PlaceType1");
		button.GrabFocus();

		//run tests
		var autoTest = new ErrorTest();
		//AddChild(autoTest);
	}

	/* Button Fuctions */

	private void _on_open_button_pressed() {
		GetNode<AnimationPlayer>("MainVBox/TerminalLevelSplit/LevelToolbarContainer/CanvasLayer/VerticalButtonTray/AnimationPlayer").Play("tray_open");
	}

	private void _on_reset_button_pressed()
	{
		//reset the step counter
		stepCount = 0;

		// Tell the global manager that we are resetting
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.Reset();
		manager.currLevel.Reset();

		//reset highlighting in terminals
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors)
		{
			editor.ClearAllHighlights();
			var existing = editor.GetNodeOrNull<Label>("ErrorLabel");
			if (existing != null)
			{
				existing.QueueFree();
			}
		}

		// reset terminal's highlighted objects
		manager.terminalContainer.UpdateSelectedTerminal();

		//refresh step count label
		stepCountLabel.AddThemeColorOverride("font_color", new Color(0.67f, 0.67f, 0.67f, 0.86f));
		UpdateStepCount();

		//delete error notice (exclamation mark) if exists/open
		/*if(errorNoticeIcon != null) {
		  errorNoticeIcon.QueueFree();
		  errorNoticeIcon = null;
		  }*/
		manager.currLevel.E.ClearErrorNotice();
	}

	//called in test script to have access to auto resetting
	private void _on_step_button_pressed() {
		//update stepCount regardless of error
		//if (stepButton.Disabled) return;
		//if(stepDisabled) return;
		stepDisabled = true;
		//stepButton.Disabled = true;
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// Tell the global manager that we are stepping
		manager.Step();

		if (manager.currLevel.movingList.Count != 0) return; // Can't step while stuff is still moving

		// on first step button press, trigger an autosave!
		if (stepCount == 0) {
			manager.SaveAutosave();

			// also stop all highlighting
			manager.terminalContainer.ClearHighlightedObjects();
		}

		stepCount++;
		UpdateStepCount();
		stepCountLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

		// Tell the global manager that we are stepping
		manager.currLevel.Step();

		//update code terminal highlighting to next one regardless of error
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");

		/*
		 * This will be moved into the step function of the scriptable objects
		 foreach (CodeEdit editor in codeEditors)
		 {
		// NOTE - FUNNY STUFF
		parser.ParseGetLine(null, null, editor.Text, stepCount, editor.Name);
		editor.HighlightLine(editor.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
		}
		*/

		//stepButton.Disabled = false;
		stepDisabled = false;
	}

	private void _on_run_button_pressed() {
		// get the current run speed (not running, 1x, 2x, submit)
		// increse t

	}

	private void _on_pause_button_pressed() {

	}

	// return to main menu button
	private void _on_exit_button_pressed()
	{

		// Get manager
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// set save data info to autosave
		manager.SetTargetLevelSave(0, -2);
		manager.SaveLevel();

		GetTree().ChangeSceneToFile("res://Scenes/MainMenu/main_menu.tscn");
	}

	private void _on_settings_button_pressed() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu").Visible = true;
	}
	private void _on_exit_menu_close_requested() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu").Visible = false;
	}

	private void _on_level_statistics_menu_close_requested() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/VBoxContainer/Level Statistics Menu").Visible = false;
	}

	private void _on_level_statistics_pressed() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/VBoxContainer/Level Statistics Menu").Visible = true;
	}

	private void _on_save_button_pressed() {

		// Don't allow saving while stepping!
		// TODO: visually indicate that system cannot save	
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		if (manager.currLevel.StepCount != 0) {
			return;
		}

		saveZero.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
		saveOne.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));
		saveTwo.AddThemeColorOverride("font_color_hover", new Color(0.8f, 0.8f, 0.8f));

		if (manager.CheckSaveData(manager.GetLevelID(), 0))
		{
			full_theme(saveZero);
			clearZero.Visible = true;
		}
		else
		{
			empty_theme(saveZero);
			clearZero.Visible = false;
		}

		if (manager.CheckSaveData(manager.GetLevelID(), 1))
		{
			full_theme(saveOne);
			clearOne.Visible = true;
		}
		else
		{
			empty_theme(saveOne);
			clearOne.Visible = false;
		}

		if (manager.CheckSaveData(manager.GetLevelID(), 2))
		{
			full_theme(saveTwo);
			clearTwo.Visible = true;
		}
		else
		{
			empty_theme(saveTwo);
			clearTwo.Visible = false;
		}

		GetNode<Window>("Window").Visible = true;
	}

	private void _on_save_0_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 0);
		manager.SaveLevel();
		full_theme(saveZero);
		clearZero.Visible = true;
	}

	private void _on_save_1_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 1);
		manager.SaveLevel();
		full_theme(saveOne);
		clearOne.Visible = true;
	}

	private void _on_save_2_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.SetTargetLevelSave(0, 2);
		manager.SaveLevel();
		full_theme(saveTwo);
		clearTwo.Visible = true;
	}

	private void _on_clear_0_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + manager.GetLevelID() + "/save0.save");
		empty_theme(saveZero);
		clearZero.Visible = false;
	}

	private void _on_clear_1_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + manager.GetLevelID() + "/save1.save");
		empty_theme(saveOne);
		clearOne.Visible = false;
	}

	private void _on_clear_2_pressed()
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		DirAccess.RemoveAbsolute("user://level" + manager.GetLevelID() + "/save2.save");
		empty_theme(saveTwo);
		clearTwo.Visible = false;
	}

	private void _on_window_close_requested()
	{
		GetNode<Window>("Window").Visible = false;
	}

	/* Helper Funcitons */

	private void UpdateStepCount()
	{
		stepCountLabel.Text = "Step Count: " + stepCount;
	}

	private void full_theme(Button button)
	{
		button.AddThemeStyleboxOverride("normal", sbf);
		button.AddThemeStyleboxOverride("hover", sbfh);
		button.AddThemeStyleboxOverride("focus", sbf);
	}

	private void empty_theme(Button button)
	{
		button.AddThemeStyleboxOverride("normal", sbe);
		button.AddThemeStyleboxOverride("hover", sbeh);
		button.AddThemeStyleboxOverride("focus", sbe);
	}

	/* Error Functions */


	/* Testing Functions */

	//called in test script to have access to auto stepping
	public void simulateStep() {
		_on_step_button_pressed();
	}

	public void simulateReset() {
		_on_reset_button_pressed();
	}

}
