using Godot;
using System;
using System.Collections.Generic;

// Tab container is the parent for all the terminals, use this to control / spawn / kill all terminals
public partial class Terminals : TabContainer
{
	// realistically nothing should exceed 15 chars but this looks better on the current screen
	private int maxLineLength = 50;
	// temp vars - remove once run buttons are established
	public int currentLine = 0;

	// dynamic list of editors
	private List<CodeEdit> editors = new();

	public override void _Ready()
	{
		currentLine = 0;

		// attach input handlers to any existing child editors
		foreach (Node child in GetChildren())
		{
			if (child is CodeEdit codeEdit)
				RegisterEditor(codeEdit);
		}

		this.TabSelected += OnTabSelected;

		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.terminalContainer = this;
		
		// run terminal selected functionality
		// TODO: but does this actually work?
		GetCurrentEditor().TerminalSelected();
	}

	public CodeEdit AddEditor(string initialText = "Your Solution Here")
	{
		CodeEdit newEditor = new CodeEdit();
		newEditor.PlaceholderText = initialText;
		AddChild(newEditor);
		editors.Add(newEditor);
		newEditor.AddToGroup("CodeTerminals");
		RegisterEditor(newEditor);
		
		return newEditor;
	}

	public void RemoveEditor(CodeEdit editor)
	{
		if (editors.Contains(editor))
		{
			editors.Remove(editor);
			editor.QueueFree();
		}
	}

	public void RemoveAllEditors()
	{
		foreach (CodeEdit editor in editors)
			editor.QueueFree();
		editors.Clear();
	}

	// attaches the input handler to enforce max line length
	private void RegisterEditor(CodeEdit codeEdit)
	{
		codeEdit.GuiInput += (InputEvent @event) => OnCodeEditInput(@event, codeEdit);
	}

	// Enforces character length requirements
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

	private void OnTabSelected(long tab)
	{
		GD.Print("Switched to tab: " + tab);
		
		
		// run terminal selected functionality
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// if last selected terminal exists, tell it to stop highlighting
		if (manager.lastSelectedTerminal != null) {
			manager.lastSelectedTerminal.StopHighlighting();
		}
		
		// update last selected terminal
		manager.lastSelectedTerminal = GetCurrentEditor();
		
		// call terminal's "just got selected" function
		GetCurrentEditor().TerminalSelected();
	}

	public CodeEdit GetCurrentEditor()
	{
		return GetChild<CodeEdit>(CurrentTab) as CodeEdit;
	}

	public List<CodeEdit> GetAllEditors()
	{
		return editors;
	}
}
