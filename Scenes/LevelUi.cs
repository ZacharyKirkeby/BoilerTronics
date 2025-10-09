using Godot;
using System;

public partial class LevelUi : Node2D
{

	private Label stepCountLabel;
	private int stepCount = 0;
	private bool isError = true; //temp boolean to track if an error has occured
	private Node2D errorNoticeIcon;
	private String[] errorTypes = {"ClawRail","ClawOutOfBounds","ClawCollision","ClawInventory"}; //keep track of current error type
	private int currError = 2; //current error type identifier (defined by errorTypes array)
	private Vector2 errorCoords = new Vector2(700,100);

	public override void _Ready() {
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		UpdateStepCount();
	}
	
	public void setError(int err) {
		if((err >= -1) && (err < 4))
		currError = err;
		isError = true;
	}
	
	public void setErrorCoords(int x, int y) {
		errorCoords = new Vector2(x,y);
	}
	
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	private void removeError() {
		isError = false;
		currError = -1;
	}

	//displays error (specific error popup, location of error on level ui, code terminals highlighted red)
	private void handleError(int errorType) {
		//open error notice (exclamation mark) at coords of error
		if(errorNoticeIcon == null) {
			var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
			errorNoticeIcon = scene.Instantiate<Node2D>();
			AddChild(errorNoticeIcon);
		}
		//TODO: replace example coords with actual (make dynamic)
		errorNoticeIcon.Position = errorCoords;

		PackedScene packedErrorScene = null;
			
		//highlight all current lines with error coloring
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors)
		{
			editor.HighlightLine(editor.getLastHighlighted() + 1, new Color(1, 0, 0, 0.3f));
		}

		//dynamic error handling
		switch(currError) {
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
		if(packedErrorScene != null) {
			var instance = packedErrorScene.Instantiate();
			GetTree().CurrentScene.AddChild(instance);
		}
	}
	
	private void _on_step_button_pressed() {
		stepCount++;
		if(!isError) {
			UpdateStepCount();
			
			stepCountLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

			//code highlighting for each terminal (shows current step)
			var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
			foreach (CodeEdit editor in codeEditors)
			{
				editor.HighlightLine(editor.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
			}
		}
		else {
			handleError(currError);
		}
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
		if(errorNoticeIcon != null) {
			errorNoticeIcon.QueueFree();
			errorNoticeIcon = null;
		}
		
		removeError();
	}
}
