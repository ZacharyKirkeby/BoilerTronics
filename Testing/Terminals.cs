using Godot;
using System;
using System.Collections.Generic;

// Tab container is the parent for all the terminals, use this to control / spawn / kill all terminals
public partial class Terminals : TabContainer
{
	// realistically nothing should exceed 15 chars but this looks better 
	private int maxLineLength = 30;
	public override void _Ready()
	{
		// handlers for each child node
		foreach (Node child in GetChildren())
		{
			if (child is CodeEdit codeEdit)
			{
				codeEdit.GuiInput += (InputEvent @event) => OnCodeEditInput(@event, codeEdit);
			}
		}
		this.TabSelected += OnTabSelected;
	}

	private void OnCodeEditInput(InputEvent @event, CodeEdit codeEdit)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			long unicode = keyEvent.Unicode;
			// Only printable characters
			if (unicode < 32)
				return;

			int caretLine = codeEdit.GetCaretLine();
			string lineText = codeEdit.GetLine(caretLine);

			if (lineText.Length > maxLineLength)
			{
			lineText = lineText.Substring(0, maxLineLength);
			codeEdit.SetLine(caretLine, lineText);
			int caretCol = Math.Min(codeEdit.GetCaretColumn(), maxLineLength);
			codeEdit.SetCaretColumn(caretCol);
			}
		}
	}
	// presently without a play button the easiest to attach to event is switching tabs
	// this is a simple proof of grabbing text from the editor
	private void OnTabSelected(long tab)
	{
		GD.Print("Switched to tab: " + tab);
		var codeEdit = GetChild<CodeEdit>((int)tab);
		GD.Print("Current text: " + codeEdit.Text);
	}

	public CodeEdit GetCurrentEditor()
	{
		return GetChild<CodeEdit>(CurrentTab) as CodeEdit;
	}
}
