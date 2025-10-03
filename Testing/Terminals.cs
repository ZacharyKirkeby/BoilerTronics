using Godot;
using System;

public partial class Terminals : TabContainer {
	public override void _Ready() {
		this.TabSelected += OnTabSelected;
	}
	private void OnTabSelected(long tab)
	{
		GD.Print("Switched to tab: " + tab);
		var codeEdit = GetChild<CodeEdit>((int)tab);
		String line = codeEdit.Text;
		GD.Print("Current text: " + line);
		// reference parser with line
	}

	public CodeEdit GetCurrentEditor() {
		return GetChild<CodeEdit>(CurrentTab) as CodeEdit;
	}
}
