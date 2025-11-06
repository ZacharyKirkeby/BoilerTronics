using BoilerTronicsObjects.Interfaces;
using Godot;
using Parsing;
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

	// Enforces character length requirements and triggers validation
	private void OnCodeEditInput(InputEvent @event, CodeEdit codeEdit)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currLevel.StepCount != 0)
			{
				return;
			}

			UpdateSelectedTerminal();
			if (HasAnySyntaxErrors())
            {
				//set error state higher up
				manager.currLevel.E.setError(true);
            }
			
			// Check if Enter/Return was pressed (new line) - trigger validation
			if (keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
			{
				CallDeferred(nameof(ValidateCurrentEditor), "newline");
			}
			
			// Check if up/down arrow (line selection change) - trigger validation
			if (keyEvent.Keycode == Key.Up || keyEvent.Keycode == Key.Down)
			{
				CallDeferred(nameof(ValidateCurrentEditor), "navigation");
			}

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

			if (HasAnySyntaxErrors())
			{
				//set error state higher up
				manager.currLevel.E.setError(true);

			}
		}
	}

	private void ValidateCurrentEditor(string trigger = "")
	{
		CodeEdit currentEditor = GetCurrentEditor();
		if (currentEditor != null)
		{
			if (trigger == "newline")
			{
				currentEditor.OnNewLine();
			}
			else if (trigger == "navigation")
			{
				currentEditor.OnLineNavigation();
			}
			else
			{
				currentEditor.ValidateAndHighlight();
			}
		}
	}

	// when a new tab is selected, run validation
	private void OnTabSelected(long tab)
	{
		var manager = BoilerTronicsGlobalManager.GlobalManager;
		if (manager?.terminalContainer == null) return;

		var terminalVBox = manager.terminalContainer.GetParent() as VBoxContainer;
		if (terminalVBox == null) return;

		var registerPanel = terminalVBox.GetNodeOrNull<PanelContainer>("RegisterPanel");
		if (registerPanel == null) return;

		var registerLabel = registerPanel.GetNodeOrNull<RegisterLabel>("RegisterLabel");
		if (registerLabel == null) return;

		var currentEditor = GetTabControl((int)tab) as CodeEdit;
		if (currentEditor == null) return;

		var obj = currentEditor.getObject();
		if (obj == null) return;
		Parser parser = null;
		if (obj is Scriptable scriptabl)
			parser = scriptabl.GetParser();
		if (parser != null)
			registerLabel.SetParser(parser);

		UpdateSelectedTerminal();
	}


	// update selected terminal; important for corresponding object highlighting!
	public void UpdateSelectedTerminal()
	{
		ClearHighlightedObjects();
		
		// call terminal's "just got selected" function
		// check: is the current editor queued for deletion? check to avoid debug errors
		if (IsInstanceValid(GetCurrentEditor()))
		{
			GetCurrentEditor().TerminalSelected();
		}
	}
	
	// tells all layers to stop highlighting objects
	public void ClearHighlightedObjects()
	{
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
		int index = CurrentTab;
		if (index < 0 || index >= GetChildCount())
		{
			return null;
		}
		
		Node child = GetChild(index);
		return child as CodeEdit;
	}

	public List<CodeEdit> GetAllEditors()
	{
		return editors;
	}
	
	// set all sub editors editable or not
	public void SetEditorsEditable(bool val)
	{
		foreach (CodeEdit panel in editors)
		{
			GD.Print("Terminals: set ", panel, " editable to ", val);
			panel.SetEditable(val);
		}
	}
	
	// Validate all editors
	public void ValidateAllEditors()
	{
		foreach (CodeEdit editor in editors)
		{
			editor.ValidateAndHighlight();
		}
	}

	// Clear all errors from all editors
	public void ClearAllErrors()
	{
		foreach (CodeEdit editor in editors)
		{
			editor.ClearAllErrors();
		}
	}
	
	// Check if any editor has syntax errors
	public bool HasAnySyntaxErrors()
	{
		foreach (CodeEdit editor in editors)
		{
			if (editor.HasErrors())
			{
				return true;
			}
		}
		return false;
	}
}