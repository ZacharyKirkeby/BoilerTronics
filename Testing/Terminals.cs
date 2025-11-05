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
		// didn't work in the first place, causes problems; disabled.
		// CodeEdit curr = GetCurrentEditor();
		// if (curr != null) { curr.TerminalSelected();}
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
			//TODO - delete should reflect change in count
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
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currLevel.StepCount != 0)
			{

				return;
			}
			// TODO: inefficient call if this runs every time the terminal at all updates!
			UpdateSelectedTerminal();

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

	// when a new tab is selected, run
	// TODO on tab selection, run error checker on both tabs
	private void OnTabSelected(long tab)
	{
		GD.Print("Switched to tab: " + tab);
		
		UpdateSelectedTerminal();
	}
	
	// update selected terminal; important for corresponding object highlighting!
	public void UpdateSelectedTerminal() {
		// run terminal selected functionality
		
		
		// if last selected terminal exists, tell it to stop highlighting
		// if (manager.lastSelectedTerminal != null) {
			// manager.lastSelectedTerminal.StopHighlighting();
		// }
		
		// update last selected terminal
		// manager.lastSelectedTerminal = GetCurrentEditor();
		
		ClearHighlightedObjects();
		
		// call terminal's "just got selected" function
		// check: is the current editor queued for deletion? check to avoid debug errors
		if (IsInstanceValid(GetCurrentEditor())) GetCurrentEditor().TerminalSelected();
	}
	
	// tells all layers to stop highlighting objects
	public void ClearHighlightedObjects() {
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		Vector2I dummy = new Vector2I(0, 0);
		
		// TODO: INEFFICIENT:
		// tells all layers to disable highlighting
		// TODO: make some basic data field in the global manager that'll keep track of the last selected highlighted layer or something like that
		if (manager.layerClaw != null) { manager.layerClaw.HighlightTile(false, dummy); }
		if (manager.layerFactory != null) {manager.layerFactory.HighlightTile(false, dummy); }
		if (manager.layerFloor != null) {manager.layerFloor.HighlightTile(false, dummy); }
		if (manager.layerMovement != null) {manager.layerMovement.HighlightTile(false, dummy); }
		if (manager.layerRail != null) {manager.layerRail.HighlightTile(false, dummy); }
	}

	public CodeEdit GetCurrentEditor()
	{
		// TODO: sometimes when deleting, or some other actions, 'CurrentTab' can go negative!
		// "Index p_index = (...) is out of bounds ((int)data.children_cache.size() - data.internal_children_front_count_cahce - data.internal_children_back_count_cache = 0)
		// int index = CurrentTab;
		// if (index < 0) { index = 0; GD.Print("Terminal: CurrentTab has a negative value: ", index);}
		return GetChild<CodeEdit>(CurrentTab) as CodeEdit;
	}

	public List<CodeEdit> GetAllEditors()
	{
		return editors;
	}
	
	// set all sub editors editable or not
	public void SetEditorsEditable(bool val) {
		foreach (CodeEdit panel in editors) {
			GD.Print("Terminals: set ", panel, " editable to ", val);
			panel.SetEditable(val);
		}
	}
}
