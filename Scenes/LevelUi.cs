using Godot;
using System;

public partial class LevelUi : Node2D
{

	private Label stepCountLabel;
	private int stepCount = 0;
	bool error = true;

	public override void _Ready() {
		stepCountLabel = GetNode<Label>("%Step Count");
		UpdateStepCount();
	}
	
	private void _on_button_pressed() {
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
	
	private void _on_step_button_pressed() {
		if(!error) {
			stepCount++;
			UpdateStepCount();
		}
	}

	private void UpdateStepCount() {
		stepCountLabel.Text = "Step Count: " + stepCount;
	}
	
	private void _on_reset_button_pressed() {
		stepCount = 0;
		UpdateStepCount();
	}
}
