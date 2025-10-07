using Godot;
using System;
using System.Collections.Generic;

using Parsing;
public partial class LevelUi : Node2D
{
	private int currentLine = 0;
	private TabContainer tabs;
	private Parser parser;
	private List<CodeEdit> editors = new();

	public override void _Ready()
	{   // adds all the current code editors to the listr
		tabs = GetNode<TabContainer>("TabContainer");
		parser = GetNode<Parser>("Parser");

		foreach (Node child in tabs.GetChildren())
		{
			if (child is CodeEdit editor)
				editors.Add(editor);
		}
		// NOTE: ADD/REMOVE OF TERMINALS NEEDS TO ALSO UPDATE THIS VAR
	}


	private void _on_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	// ideally where this is handled but for now placeholder
	private void idk()
	{
		foreach (var editor in editors)
		{
			// sends the whole text sadly, will need to process
			parser.ParseGetLine(editor.Text, currentLine);
		}
	}
}
