using Godot;
using System;

public partial class LevelUi : Node2D
{

	private Label stepCountLabel;
	private int stepCount = 0;
	bool error = false; //temp boolean to track if an error has occured
	private Node2D errorNoticeIcon;

	public override void _Ready() {
		stepCountLabel = GetNode<Label>("%Step Count"); //unique identifier for the step counter
		UpdateStepCount();
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
		}
		else {
			//open error notice (exclamation mark) at coords of error
			if(errorNoticeIcon == null) {
				var scene = (PackedScene)ResourceLoader.Load("res://Resources/ErrorNotice.tscn");
				errorNoticeIcon = scene.Instantiate<Node2D>();
				AddChild(errorNoticeIcon);
				//TODO: replace example coords with actual (make dynamic)
				errorNoticeIcon.Position = new Vector2(1000, 600);
			}

			//TODO: replace example with actual error (make dynamic)
			var packedErrorScene = ResourceLoader.Load<PackedScene>("Resources/ClawRailError.tscn");
			var instance = packedErrorScene.Instantiate();
			GetTree().CurrentScene.AddChild(instance);
		}
	}

	private void UpdateStepCount() {
		stepCountLabel.Text = "Step Count: " + stepCount;
	}
	
	private void _on_reset_button_pressed() {
		stepCount = 0;
		stepCountLabel.AddThemeColorOverride("font_color", new Color(0.67f, 0.67f, 0.67f, 0.86f));
		UpdateStepCount();
		//delete error notice (exclamation mark) if exists/open
		if(errorNoticeIcon != null) {
			errorNoticeIcon.QueueFree();
			errorNoticeIcon = null;
		}
	}
}
