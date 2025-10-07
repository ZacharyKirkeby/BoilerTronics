using Godot;
using System;

public partial class LevelUi : Node2D
{

	private Label stepCountLabel;
	private int stepCount = 0;
	bool error = true; //temp boolean to track if an error has occured
	private Node2D errorNoticeIcon;
	private String[] errorTypes = {"ClawRail","ClawOutOfBounds","ClawCollision","ClawInventory"}; //keep track of current error type
	private int currError = 2; //current error type identifier (defined by errorTypes array)
	private Vector2 errorCoords = new Vector2(700,100);

	public override void _Ready() {
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		UpdateStepCount();
	}
	
	public void setCurrError(int err) {
		if((err >= -1) && (err < 4))
		currError = err;
	}
	
	public void setErrorCoords(int x, int y) {
		errorCoords = new Vector2(x,y);
	}
	
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
	
	private void _on_step_button_pressed() {
		if(!error) {
			stepCount++;
			UpdateStepCount();
			
			//TODO: dynamic text coloring
			stepCountLabel.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

			//Code Highlighting
			var editor2 = GetNode<CodeEdit>("MainVBox/TerminalLevelSplit/TerminalContainer/CodeEdit2");
			editor2.HighlightLine(stepCount % editor2.GetLineCount() - 1);
		}
		else {
			//open error notice (exclamation mark) at coords of error
			if(errorNoticeIcon == null) {
				var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
				errorNoticeIcon = scene.Instantiate<Node2D>();
				AddChild(errorNoticeIcon);
				//TODO: replace example coords with actual (make dynamic)
				errorNoticeIcon.Position = errorCoords;
			}
			PackedScene packedErrorScene = null;

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
					break; //should not happen
			}
			if(packedErrorScene != null) {
				var instance = packedErrorScene.Instantiate();
				GetTree().CurrentScene.AddChild(instance);
			}
		}
	}

	private void UpdateStepCount() {
		stepCountLabel.Text = "Step Count: " + stepCount;
	}
	
	private void _on_reset_button_pressed() {
		stepCount = 0;
		var editor2 = GetNode<CodeEdit>("MainVBox/TerminalLevelSplit/TerminalContainer/CodeEdit2");
		editor2.ClearAllHighlights();
		stepCountLabel.AddThemeColorOverride("font_color", new Color(0.67f, 0.67f, 0.67f, 0.86f));
		UpdateStepCount();
		//delete error notice (exclamation mark) if exists/open
		if(errorNoticeIcon != null) {
			errorNoticeIcon.QueueFree();
			errorNoticeIcon = null;
		}
		error = false;
		setCurrError(-1);
	}
}
