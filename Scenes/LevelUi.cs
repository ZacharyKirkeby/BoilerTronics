using Godot;
using System;
using System.Collections.Generic;

using Parsing;
public partial class LevelUi : Node2D
{
	private int currentLine = 0;
	private TabContainer tabs;
	private Parser parser;
	private List<CodeEdit> editors = new();
	// automatically define the global manager so we don't need to keep redefining it and etc
	static BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

	private Label stepCountLabel;
	private int stepCount = 0;
	private bool isError = false; //temp boolean to track if an error has occured
	private Node2D errorNoticeIcon;
	private String[] errorTypes = { "ClawRail", "ClawOutOfBounds", "ClawCollision", "ClawInventory" }; //keep track of current error type
	private int errorID = -1; //current error type identifier (defined by errorTypes array)
	private Vector2 errorCoords = new Vector2(700, 100);
	private String errorEditor;
	private StyleBoxFlat sbf = new StyleBoxFlat();
	private StyleBoxFlat sbe = new StyleBoxFlat();
	private StyleBoxFlat sbeh = new StyleBoxFlat();
	private StyleBoxFlat sbfh = new StyleBoxFlat();
	private Button saveZero;
	private Button saveOne;
	private Button saveTwo;
	private Button clearZero;
	private Button clearOne;
	private Button clearTwo;

	public override void _Ready()
	{
		tabs = GetNode<TabContainer>("/root/Node2D/MainVBox/TerminalLevelSplit/TerminalContainer");

		parser = GetNode<Parser>("/root/Node2D/MainVBox/TerminalLevelSplit/Parser");
		parser.Connect(Parser.SignalName.ErrorRaised, new Callable(this, nameof(OnParserErrorRaised)));

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
		UpdateStepCount();
		// manager.SetDraggable(false); // debug; testing script
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.P = parser;
	}

	//set error status as true with errorID and name of terminal causing error
	public void setError(int errID, String editor)
	{
		if ((errID >= -1) && (errID < 4))
			errorID = errID;
		isError = true;
		errorEditor = editor;
	}

	public void setErrorCoords(int x, int y)
	{
		errorCoords = new Vector2(x, y);
	}

	// return to main menu button
	private void _on_button_pressed()
	{

		// Get manager
		// BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// set save data info to autosave
		manager.SetTargetLevelSave(0, -2);
		manager.SaveLevel();

		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	private void _on_save_button_pressed() {
		
		// Don't allow saving while stepping!
		// TODO: visually indicate that system cannot save	
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

	private void removeError()
	{
		isError = false;
		errorID = -1;
	}

	//displays error (specific error popup, location of error on level ui, specific code terminal highlighted red)
	private void handleError(int errorType, String badEditor)
	{
		//open error notice (exclamation mark) at coords of error
		//TODO: add this to camera2D in actual level window
		if (errorNoticeIcon == null)
		{
			var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
			errorNoticeIcon = scene.Instantiate<Node2D>();
			AddChild(errorNoticeIcon);
		}
		//TODO: replace example coords with actual (make dynamic)
		errorNoticeIcon.Position = errorCoords;

		PackedScene packedErrorScene = null;

		//highlight current line of erroneous terminal red
		//TODO: ensure name is what we end up differentiating by
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		CodeEdit terminal = null;
		foreach (CodeEdit editor in codeEditors)
		{
			if (editor.Name == badEditor)
			{
				terminal = editor;
			}
		}
		terminal.HighlightLine(terminal.getLastHighlighted(), new Color(1, 0, 0, 0.3f));

		//dynamic error popups based on type of error
		switch (errorID)
		{
			case 0:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Resources/ClawRailError.tscn");
				break;
			case 1:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Resources/ClawOutOfBoundsError.tscn");
				break;
			case 2:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Resources/ClawCollisionError.tscn");
				break;
			case 3:
				packedErrorScene = ResourceLoader.Load<PackedScene>("res://Resources/ClawInventoryError.tscn");
				break;
			case 4:
				// no popup
				break;
			case -1:
				break; //should not happen as error should be set to false
		}

		// actually display error notice
		if (packedErrorScene != null)
		{
			var instance = packedErrorScene.Instantiate();
			GetTree().CurrentScene.AddChild(instance);
		}
	}

	private void _on_step_button_pressed()
	{
		//update stepCount regardless of error
		if (!isError)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
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
		}

		//if error, handle accordingly with popups and code terminal highlighting
		if (isError)
		{
			handleError(errorID, errorEditor);
		}
	}

	private void UpdateStepCount()
	{
		stepCountLabel.Text = "Step Count: " + stepCount;
	}

	private void _on_reset_button_pressed()
	{
		//reset the step counter
		stepCount = 0;

		// Tell the global manager that we are resetting
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
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
		if (errorNoticeIcon != null)
		{
			errorNoticeIcon.QueueFree();
			errorNoticeIcon = null;
		}

		removeError();
	}

	private void OnParserErrorRaised(int lineNumber, string message, string editorName)
	{
		// Prevent stepping while error exists
		setError(4, editorName);

		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors)
		{
			if (editor.Name == editorName)
			{
				var existing = editor.GetNodeOrNull<Label>("ErrorLabel");
				if (existing != null)
				{
					existing.QueueFree();
				}

				// Create error label
				Label errorLabel = new Label
				{
					Name = "ErrorLabel",
					Text = $"Line {lineNumber}: {message}",
					HorizontalAlignment = HorizontalAlignment.Left,
					Modulate = new Color(1, 0.3f, 0.3f),
					AutowrapMode = TextServer.AutowrapMode.Word
				};

				// Add to bottom of CodeEdit
				editor.AddChild(errorLabel);
				errorLabel.AnchorLeft = 0;
				errorLabel.AnchorRight = 1;
				errorLabel.AnchorBottom = 1;
				errorLabel.AnchorTop = 1;
				errorLabel.OffsetBottom = -4;

				errorLabel.Position = new Vector2(0, editor.Size.Y - 20);

				// Highlight error line
				editor.HighlightLine(lineNumber, new Color(1, 0, 0, 0.25f));

				//TODO - delete
				GD.Print($"[ParserError] {editorName}: Line {lineNumber} -> {message}");
				break;
			}
		}
	}
}
