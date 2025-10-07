using Godot;
using System;
using System.Collections.Generic;


public partial class LevelUi : Node2D
{
	private TabContainer tabs;
	private Parser parser;
	private List<CodeEdit> editors = new();

	public override void _Ready()
	{
		tabs = GetNode<TabContainer>("TabContainer");
		parser = GetNode<Parser>("Parser");

		foreach (Node child in tabs.GetChildren())
		{
			if (child is CodeEdit editor)
				editors.Add(editor);
		}
	}


	private void _on_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
