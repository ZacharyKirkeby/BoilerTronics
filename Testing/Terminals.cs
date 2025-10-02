using Godot;
using System;

public partial class Terminals : TabContainer {
	public override void _Ready() {
		this.TabSelected += OnTabSelected;
	}
	private void OnTabSelected(long tab) {
		GD.Print("Switched to tab: " + tab);
		// Access the CodeEdit in that tab
		var codeEdit = GetChild<CodeEdit>((int)tab);
		GD.Print("Current text: " + codeEdit.Text);
	}

	public CodeEdit GetCurrentEditor() {
		return GetChild<CodeEdit>(CurrentTab) as CodeEdit;
	}
}
