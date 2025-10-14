using Godot;
using System;

public partial class LevelUi : Node2D
{

	private Label stepCountLabel;
	private int stepCount = 0;
	private bool isError = false; //temp boolean to track if an error has occured
	private Node2D errorNoticeIcon;
	private String[] errorTypes = {"ClawRail","ClawOutOfBounds","ClawCollision","ClawInventory"}; //keep track of current error type
	private int errorID = -1; //current error type identifier (defined by errorTypes array)
	private Vector2 errorCoords = new Vector2(100,200);
	private String errorEditor;
	private Button stepButton;
	private Node errorSceneInstance;

	public override void _Ready() {
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		stepButton = GetNode<Button>("%Step Button");
		UpdateStepCount();
		
		//run tests
		var autoTest = new ErrorTest();
		AddChild(autoTest);;
	}
	
	public void RemoveErrorScene() {
		if(IsInstanceValid(errorSceneInstance)) {
			errorSceneInstance.QueueFree();
			errorSceneInstance = null;
			GD.Print("Error scene removed.");
		}
		else {
			GD.Print("Error scene failed to removed.");
		}
	}
	
	//set error status as true with errorID and name of terminal causing error
	public void setError(int errID, String editor) {
		if((errID >= -1) && (errID < 4))
		errorID = errID;
		isError = true;
		errorEditor = editor;
	}
	
	public void setErrorCoords(int x, int y) {
		errorCoords = new Vector2(x,y);
	}
	
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	private void removeError() {
		isError = false;
		errorID = -1;
	}
	
	//be able to call for error popup from this script
	private void ShowErrorNotice(Vector2 position) {
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.SpawnErrorSprite(position);
	}
	
	//be able to call for error popup removal from this script
	private void ClearErrorNotice() {
		var camera = GetTree().CurrentScene.GetNode<BoilerTronicsObjects.GameCamera.Camera2d>("MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport/Node2D/Camera2D");
		camera.RemoveErrorSprite();
	}

	//displays error (specific error popup, location of error on level ui, specific code terminal highlighted red)
	private void handleError(int errorType, String badEditor) {
		//open error notice (exclamation mark) at coords of error
		//TODO: add this to camera2D in actual level window
		/*if(errorNoticeIcon == null) {
			var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
			errorNoticeIcon = scene.Instantiate<Node2D>();
			AddChild(errorNoticeIcon);
		}
		//TODO: replace example coords with actual (make dynamic)
		errorNoticeIcon.Position = errorCoords;*/
		ShowErrorNotice(errorCoords);

		PackedScene packedErrorScene = null;
			
		//highlight current line of erroneous terminal red
		//TODO: ensure name is what we end up differentiating by
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		CodeEdit terminal = null;
		foreach (CodeEdit editor in codeEditors)
		{
			if(editor.Name == badEditor) {
				terminal = editor;
			}
		}
		terminal.HighlightLine(terminal.getLastHighlighted(), new Color(1, 0, 0, 0.3f));

		//dynamic error popups based on type of error
		switch(errorID) {
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
			case -1:
				break; //should not happen as error should be set to false
		}
		
		//actually display error notice
		if(packedErrorScene != null) {
			errorSceneInstance = packedErrorScene.Instantiate();
			GetTree().CurrentScene.AddChild(errorSceneInstance);
		}
	}
	
	//called in test script to have access to auto stepping
	public void simulateStep() {
		_on_step_button_pressed();
	}
	
	public void simulateReset() {
		_on_reset_button_pressed();
	}
	
	//called in test script to have access to auto resetting
	private void _on_step_button_pressed() {
		//update stepCount regardless of error
		stepButton.Disabled = true;
		if(!isError) {
			stepCount++;
			UpdateStepCount();
			stepCountLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

			//update code terminal highlighting to next one regardless of error
			var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
			foreach (CodeEdit editor in codeEditors)
			{
				editor.HighlightLine(editor.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
			}
			//TODO: check for actual error and use setError to properly display error notices
			//example error being manually set after third step
			if(stepCount == 3) {
				setError(2, "CodeEdit2");
			}
		}
		
		//if error, handle accordingly with popups and code terminal highlighting
		if(isError) {
			handleError(errorID, errorEditor);
		}
		stepButton.Disabled = false;
	}

	private void UpdateStepCount() {
		stepCountLabel.Text = "Step Count: " + stepCount;
	}
	
	private void _on_reset_button_pressed() {
		//reset the step counter
		stepCount = 0;

		//reset highlighting in terminals
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors)
		{
			editor.ClearAllHighlights();
		}

		//refresh step count label
		stepCountLabel.AddThemeColorOverride("font_color", new Color(0.67f, 0.67f, 0.67f, 0.86f));
		UpdateStepCount();

		//delete error notice (exclamation mark) if exists/open
		/*if(errorNoticeIcon != null) {
			errorNoticeIcon.QueueFree();
			errorNoticeIcon = null;
		}*/
		ClearErrorNotice();
		
		removeError();
	}
}
