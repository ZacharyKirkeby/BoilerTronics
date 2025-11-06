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
	private Button pauseButton;
	private Button playButton;

	/* Icons */
	private Texture2D playIcon;
	private Texture2D submitIcon;

	public override void _Ready()
	{
		tabs = GetNode<TabContainer>("/root/Node2D/MainVBox/TerminalLevelSplit/TerminalVBox/TerminalContainer");


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
		pauseButton = GetNode<Button>("%Pause Button");
		playButton = GetNode<Button>("%Play Button");

		// manager.SetDraggable(false); // debug; testing script
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.E = new ErrorHandler();
		AddChild(manager.currLevel.E); // Add as child so that we can access elements in the level

		parser = GetNode<Parser>("/root/Node2D/MainVBox/TerminalLevelSplit/Parser");

		manager.currLevel.P = parser;

		var button = GetNode<Button>("MainVBox/PanelContainer/HBoxContainer/CategoryPicker/PlaceType1");
		button.GrabFocus();

		// Init icons for running and submittingnull
		playIcon = GD.Load<Texture2D>("res://Resources/Icons/play.png");
		submitIcon = GD.Load<Texture2D>("res://Resources/Icons/submission-speed.png");

		//run tests
		var autoTest = new ErrorTest();
		//AddChild(autoTest);
		
		// Set fullscreen toggle
		var fullscreenButton = GetNode<Button>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/Settings Menu/VBoxContainer/VBoxContainer2/Fullscreen");
		fullscreenButton.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen
			|| DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

		// Set volume slider
		BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
		var volSlider = GetNode<HSlider>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/Settings Menu/VBoxContainer/VBoxContainer2/MainVolSlider");
		volSlider.Value = soundManager.GetCurrentVolume();

		// Starts on first hint
		GetNode<Button>("%HintBack").Visible = false;

	}

	public override void _Process(double delta) {
		// Always update step count (this is for running)
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		UpdateStepCount(manager.currLevel.StepCount);

	}

	/* Button Functions */

	private void _on_open_button_pressed() {
		GetNode<AnimationPlayer>("MainVBox/TerminalLevelSplit/LevelToolbarContainer/CanvasLayer/VerticalButtonTray/AnimationPlayer").Play("tray_open");
	}
	
	private void _on_visibility_button_pressed() {
		GetNode<Window>("VisibilityWindow").Visible = true;
	}

	private void _on_reset_button_pressed()
	{
		// Tell the global manager that we are resetting
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;

		soundManager.StopAllSound();
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

		manager.currLevel.E.ClearErrorNotice();

		// TODO: reset the play button
		playButton.Text = ""; // Remove text
		playButton.Icon = playIcon;
	}
	//called in test script to have access to auto resetting
	private void _on_step_button_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// on first step button press, trigger an autosave!
		if (manager.currLevel.StepCount == 0) {
			manager.SaveAutosave();

			// also stop all highlighting
			manager.terminalContainer.ClearHighlightedObjects();
		}

		// Tell the global manager that we are stepping
		manager.Step(); // This will also call step on the level

		stepCountLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

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
	}

	private void _on_run_button_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		manager.currLevel.IncRun(); // This will call run and increase the run speed

		switch (manager.currLevel.GetGameRunState()) {
			case BoilerTronicsLevel.GameRunState.SlowRun:
				// 1X
				playButton.Text = "1X";
				playButton.Icon = null;
				break;
			case BoilerTronicsLevel.GameRunState.FastRun:
				// 2X
				playButton.Text = "2X";
				playButton.Icon = null;
				break;
			case BoilerTronicsLevel.GameRunState.SubmitSpeed:
				// Submit speed
				playButton.Text = "";
				playButton.Icon = submitIcon; // This will be the submit speed
				break;
		}
	}

	private void _on_pause_button_pressed() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.Pause(); // Pauses
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
	
	private void _on_visibility_window_close_requested() {
		GetNode<Window>("VisibilityWindow").Visible = false;
	}

	private void _on_level_statistics_pressed() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/VBoxContainer/Level Statistics Menu").Visible = true;
	}
	
	private void _on_edit_settings_button_pressed() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/Settings Menu").Visible = true;
	}
	
	private void _on_settings_menu_close_requested() {
		GetNode<Window>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/Settings Menu").Visible = false;
	}
	
	private void _on_fullscreen_toggled(bool toggledOn)
	{
		if (toggledOn)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		else
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
	}

	private void _on_main_vol_slider_value_changed(float val)
	{
		BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
		soundManager.SetCurrentVolume(val);
	}
	
	private void _on_mute_pressed()
	{
		//move slider to 0
		var slider = GetNode<HSlider>("MainVBox/TerminalLevelSplit/VBoxContainer/PanelContainer/HBoxContainer/HBoxContainer/Exit Menu/Settings Menu/VBoxContainer/VBoxContainer2/MainVolSlider");
		slider.Value = 0;
		
		//actually make volume 0
		_on_main_vol_slider_value_changed(0);
	}

	private void _on_movement_visibility_toggled(bool toggled_on)
	{
		GD.Print(toggled_on);
		if (toggled_on)
		{
			BoilerTronicsGlobalManager.GlobalManager.layerMovement.Visible = true;
		}
		else
		{
			BoilerTronicsGlobalManager.GlobalManager.layerMovement.Visible = false;
		}
	}

	private void _on_factory_visibility_toggled(bool toggled_on)
	{
		GD.Print(toggled_on);
		if (toggled_on)
		{
			BoilerTronicsGlobalManager.GlobalManager.layerFactory.Visible = true;
			BoilerTronicsGlobalManager.GlobalManager.layerFloor.Visible = true;
		}
		else
		{
			BoilerTronicsGlobalManager.GlobalManager.layerFactory.Visible = false;
			BoilerTronicsGlobalManager.GlobalManager.layerFloor.Visible = false;
		}
	}
	
	private void _on_claw_visibility_toggled(bool toggled_on)
	{
		GD.Print(toggled_on);
		if (toggled_on)
		{
			BoilerTronicsGlobalManager.GlobalManager.layerClaw.Visible = true;
			BoilerTronicsGlobalManager.GlobalManager.layerRail.Visible = true;
		}
		else
		{
			BoilerTronicsGlobalManager.GlobalManager.layerClaw.Visible = false;
			BoilerTronicsGlobalManager.GlobalManager.layerRail.Visible = false;
		}
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

	private void UpdateStepCount(int stepCount)
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

	private void _on_hints_pressed()
	{
		GetNode<Window>("%HintsWindow").Visible = true;
	}
	private void _on_hints_window_close_requested()
	{
		GetNode<Window>("%HintsWindow").Visible = false;
	}
	private void _on_hint_back_pressed()
	{
		/* TODO: Zach if this causes u to be on hint one make button invisible 
				populate last hint */
	}
	private void _on_hint_forward_pressed()
	{
		/* TODO: Zach if this causes u to be on last hint make button invisible
			populate next hint */
		
	}
	/* Testing Functions */

	//called in test script to have access to auto stepping
	public void simulateStep() {
		_on_step_button_pressed();
	}

	public void simulateReset() {
		_on_reset_button_pressed();
	}

}
