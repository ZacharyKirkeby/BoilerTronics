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
	private Label costCountLabel;
	
	/* Statistics */
	private Label ppsCutoffLabel;
	private Label cpsCutoffLabel;
	private Label rcCutoffLabel;
	
	private Label ppsSolutionLabel;
	private Label cpsSolutionLabel;
	private Label rcSolutionLabel;
	
	private Label ppsGradeLabel;
	private Label cpsGradeLabel;
	private Label rcGradeLabel;
	
	private Label ppsDifferenceLabel;
	private Label cpsDifferenceLabel;
	private Label rcDifferenceLabel;

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

	public override void _Ready()
	{
		GD.Print(GetPath());
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
		
		costCountLabel = GetNode<Label>("%Cost Count");
		
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		stepButton = GetNode<Button>("%Step Button");
		
		/* Statistics Labels */
		ppsCutoffLabel = GetNode<Label>("%PPS Cutoff");
		ppsSolutionLabel = GetNode<Label>("%PPS Solution");
		ppsGradeLabel = GetNode<Label>("%PPS Grade");
		ppsDifferenceLabel = GetNode<Label>("%PPS Difference");
		
		cpsCutoffLabel = GetNode<Label>("%CPS Cutoff");
		cpsSolutionLabel = GetNode<Label>("%CPS Solution");
		cpsGradeLabel = GetNode<Label>("%CPS Grade");
		cpsDifferenceLabel = GetNode<Label>("%CPS Difference");
		
		rcCutoffLabel = GetNode<Label>("%RC Cutoff");
		rcSolutionLabel = GetNode<Label>("%RC Solution");
		rcGradeLabel = GetNode<Label>("%RC Grade");
		rcDifferenceLabel = GetNode<Label>("%RC Difference");
		
		UpdateStepCount(0);
		// manager.SetDraggable(false); // debug; testing script
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		UpdateCost(manager.currLevel.cost);
		SetStatisticDefaults();
		manager.currLevel.E = new ErrorHandler();
		AddChild(manager.currLevel.E); // Add as child so that we can access elements in the level

		parser = GetNode<Parser>("/root/Node2D/MainVBox/TerminalLevelSplit/Parser");
		parser.Connect(Parser.SignalName.ErrorRaised, new Callable(manager.currLevel.E, nameof(manager.currLevel.E.OnParserErrorRaised)));

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
		// Tell the global manager that we are resetting
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		manager.Reset();
		manager.currLevel.Reset();

		//reset statistics as solution is wiped
		SetStatisticDefaults();
		manager.currLevel.ResetSolutionStats();

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

		UpdateStepCount(manager.currLevel.StepCount);

		manager.currLevel.E.ClearErrorNotice();
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

		UpdateStepCount(manager.currLevel.StepCount); // Updates the step count

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
		// TODO: Implement
		// On press we should look at our current run state
		// If we have paused or are stepping, don't do anything
		// If we are not running, go to 1x
		// If we are at 1x, go to 2x
		// If we are at 2x, go to submit speed
		// If we are at submit speed, don't do anything
	}

	private void _on_pause_button_pressed() {
		// If we are not running, don't do anything
		// Otherwise, stop running
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

	private void UpdateStepCount(int stepCount)
	{
		stepCountLabel.Text = "Step Count: " + stepCount;
	}
	
	public void UpdateCost(int cost) {
		if (costCountLabel == null) {
			return;
		}
		costCountLabel.Text = "Cost: $" + cost;
		GD.Print("cost updated" + cost);
	}
	
	public void SetStatisticDefaults() {
		ppsSolutionLabel.Text = "PPS: N/A";
		cpsSolutionLabel.Text = "CPS: N/A";
		rcSolutionLabel.Text = "RC: N/A";
		
		ppsDifferenceLabel.Text = "Diff: N/A";
		cpsDifferenceLabel.Text = "Diff: N/A";
		rcDifferenceLabel.Text = "Diff: N/A";
		
		ppsGradeLabel.Text = "Grade: N/A";
		cpsGradeLabel.Text = "Grade: N/A";
		rcGradeLabel.Text = "Grade: N/A";
	}
	
	public void UpdateSolutionStatistics(float pps, float cps, int rc) {
		ppsSolutionLabel.Text = "PPS: " + pps.ToString("F2");
		cpsSolutionLabel.Text = "CPS: $" + cps.ToString("F2");
		rcSolutionLabel.Text = "RC: " + rc.ToString("F2");
	}
	
	public void UpdateSolutionCutoffs(float pps, float cps, int rc) {
		ppsCutoffLabel.Text = "Cutoff PPS: " + pps.ToString("F2");
		cpsCutoffLabel.Text = "Cutoff CPS: $" + cps.ToString("F2");
		rcCutoffLabel.Text = "Cutoff RC: " + rc.ToString("F2");
	}

	public void UpdateSolutionGrading(float ppsCutoff, float ppsSol, float cpsCutoff, float cpsSol, int rcCutoff, int rcSol) {
		//difference and grading labels
		if(ppsCutoff <= ppsSol) {
			//good
			ppsDifferenceLabel.Text = "Diff: " + (ppsSol - ppsCutoff).ToString("F2");
			ppsGradeLabel.Text = "Grade: " + CalculateGrade(ppsCutoff, ppsSol, true).ToString("F2");
			ppsDifferenceLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			ppsGradeLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
		}
		else {
			//bad
			ppsDifferenceLabel.Text = "Diff: " + (ppsCutoff - ppsSol).ToString("F2");
			ppsGradeLabel.Text = "Grade: " + CalculateGrade(ppsCutoff, ppsSol, false).ToString("F2");
			ppsDifferenceLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
			ppsGradeLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
		}
		if(cpsCutoff >= cpsSol) {
			//good
			cpsDifferenceLabel.Text = "Diff: " + (cpsCutoff - cpsSol).ToString("F2");
			cpsGradeLabel.Text = "Grade: " + CalculateGrade(cpsCutoff, cpsSol, true).ToString("F2");
			cpsDifferenceLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			cpsGradeLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
		}
		else {
			//bad
			cpsDifferenceLabel.Text = "Diff: " + (cpsSol - cpsCutoff).ToString("F2");
			cpsGradeLabel.Text = "Grade: " + CalculateGrade(cpsCutoff, cpsSol, false).ToString("F2");
			cpsDifferenceLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
			cpsGradeLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
		}
		if(rcCutoff >= rcSol) {
			//good
			rcDifferenceLabel.Text = "Diff: " + (rcCutoff - rcSol).ToString("F2");
			rcGradeLabel.Text = "Grade: " + CalculateGrade(rcCutoff, rcSol, true).ToString("F2");
			rcDifferenceLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			rcGradeLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
		}
		else {
			//bad
			rcDifferenceLabel.Text = "Diff: " + (rcSol - rcCutoff).ToString("F2");
			rcGradeLabel.Text = "Grade: " + CalculateGrade(rcCutoff, rcSol, false).ToString("F2");
			rcDifferenceLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
			rcGradeLabel.AddThemeColorOverride("font_color", new Color(1, .2f, .5f));
		}
	}

	public float CalculateGrade(float cutoff, float solution, bool good) {
		//check for dividing by 0
		if (cutoff == 0) {
			return 0;
		}

		float ratio = solution / cutoff;
		float grade;

		if (good) {
			if (ratio < 1.0f) {
				grade = 100f * ratio;
			}
			else {
				grade = 100f + 20f * (float)Math.Log10(ratio);
			}
		}
		else {
			if (ratio > 1.0f) {
				grade = 100f / ratio;
			}
			else {
				grade = 100f + 20f * (float)Math.Log10(1f / ratio);
			}
		}

		//ensure number is valid
		if (grade < 0f) {
			grade = 0f;
		}
		if (grade > 150f) {
			grade = 150f;
		}
		return grade;
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

	/* Testing Functions */

	//called in test script to have access to auto stepping
	public void simulateStep() {
		_on_step_button_pressed();
	}

	public void simulateReset() {
		_on_reset_button_pressed();
	}

}
