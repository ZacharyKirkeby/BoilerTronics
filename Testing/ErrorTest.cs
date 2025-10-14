using Godot;
using System;

public partial class ErrorTest : Node
{
	private LevelUi levelUi;
	
	public async override void _Ready() {
		GD.Print("Automatic Error Tests Started");

		levelUi = GetTree().CurrentScene as LevelUi;
		FillTerminals();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		RunSteps(3);
		await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
		RemoveErrorScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		ResetScene();
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		RunSteps(3);
	}

	private void FillTerminals() {
		var codeEditors = GetTree().GetNodesInGroup("CodeTerminals");

		foreach (CodeEdit editor in codeEditors) {
			//custom code per editor
			editor.Text = $"test text \n second line";
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
