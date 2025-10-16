using Godot;
using System;

public partial class ErrorTest : Node
{
	private LevelUi levelUi;
	
	public async override void _Ready() {
		GD.Print("Automatic Error Tests Started");
		await ToSignal(GetTree().CreateTimer(10.0f), "timeout");
		levelUi = GetTree().CurrentScene as LevelUi;
		ClawCollisionText();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		RunSteps(1);
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
		RemoveErrorScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		ResetScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		OutOfBoundsText();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		RunSteps(1);
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
		RemoveErrorScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		ResetScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		ClawOffRailText();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		RunSteps(1);
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
		RemoveErrorScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		ResetScene();
	}

	//claw at left edge tries to move left
	private void OutOfBoundsText() {
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors) {
			editor.Text = $"mov l";
		}
	}
	
	//claw at left edge and one to the right of it, left one will hit right
	private void ClawCollisionText() {
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		int index = 0;
		foreach (CodeEdit editor in codeEditors) {
			if(index == 0) {
				editor.Text = $"mov r";
			}
			index++;
		}
	}
	
	//claw on elft edge with rail under, will try to move down (no rail under)
	private void ClawOffRailText() {
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");
		foreach (CodeEdit editor in codeEditors) {
			editor.Text = $"mov d";
		}
	}

	private async void RunSteps(int count) {
		for (int i = 0; i < count; i++) {
			levelUi.simulateStep();
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout"); //delay between steps
		}
	}
	
	public void RemoveErrorScene() {
		levelUi.RemoveErrorScene();
	}
	
	public void ResetScene() {
		levelUi.simulateReset();
	}
}
