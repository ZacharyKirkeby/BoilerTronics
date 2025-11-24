using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using Parsing;

// Extended CodeEdit with real-time error checking
public partial class CodeEdit : Godot.CodeEdit
{
	private int lastHighlightedLine = -1;
	public int currentLine = 0;
	private int lastLine = 0;

	// Error checking fields
	private Label errorLabel;
	private Color errorColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);
	private Dictionary<int, string> lineErrors = new();
	public bool error = false;
	private bool isDirty = false;
	
	// Object highlighting fields
	private PlaceableObject correspondingObject;
	private Layer highlightedLayer;
	private bool highlightingObject = false;

	public override void _Ready()
	{
		AddToGroup("CodeTerminals");
		HighlightCurrentLine = true;
		CaretBlink = true;
		TextChanged += OnTextChanged;
		currentLine = 0;
		
		// Create error label at bottom of terminal
		CreateErrorLabel();
		
		// Initial validation
		ValidateAndHighlight();
		
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		if (manager.terminalContainer.GetCurrentEditor() == this)
		{
			manager.terminalContainer.UpdateSelectedTerminal();
		}
	}

	private void CreateErrorLabel()
	{
		errorLabel = new Label
		{
			Name = "SyntaxErrorLabel",
			HorizontalAlignment = HorizontalAlignment.Left,
			Modulate = new Color(1, 0.3f, 0.3f),
			AutowrapMode = TextServer.AutowrapMode.Word,
			Visible = false
		};
		
		AddChild(errorLabel);
		errorLabel.AnchorLeft = 0;
		errorLabel.AnchorRight = 1;
		errorLabel.AnchorBottom = 1;
		errorLabel.AnchorTop = 1;
		errorLabel.OffsetBottom = -4;
	}

	private void OnTextChanged()
	{
		GD.Print($"[{Name}] content changed:\n{Text}");
		isDirty = true;
		CallDeferred(nameof(ValidateAndHighlight));
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.E.setSyntaxError(this.error);
	}

	public PlaceableObject getObject()
	{
		return this.correspondingObject;
	}
	
	// custom function, called by Terminals.cs
	// intention is to use this function to check if 'correspondingObject' exists
	// if so, try and "highlight" the object
	// TODO: current Terminals.cs implementation doesn't call this properly on initial level creation
	public void TerminalSelected() {
		GD.Print("terminal selected");
		
		// Always validate when selected
		ValidateAndHighlight();
		
		TryHighlightingObject();
	}

	// Called when user presses Enter (new line)
	public void OnNewLine()
	{
		ValidateAndHighlight();
	}

	// Called when user navigates lines (Up/Down arrows)
	public void OnLineNavigation()
	{
		ValidateAndHighlight();
	}

	public void OnDeletion()
	{
		ValidateAndHighlight();
	}

	// Main validation and highlighting logic
	public void ValidateAndHighlight()
	{
		ClearSyntaxErrorHighlights();
		lineErrors.Clear();

		string code = this.Text;
		var errors = ProgramValidator.ValidateProgram(code);
		if (errors != null && errors.Count > 0)
		{
			this.error = true;
		} else
		{
			this.error = false;
		}
		// Process each error
		foreach (var (lineNum, errorMsg) in errors)
		{
			// Store error for this line
			if (!lineErrors.ContainsKey(lineNum))
			{
				lineErrors[lineNum] = errorMsg;
			}

			// Highlight the line with error
			HighlightSyntaxError(lineNum, errorColor);
		}

		// Update error display
		UpdateErrorLabel();
		isDirty = false;
	}

	private void ClearSyntaxErrorHighlights()
	{
		// Only clear syntax error highlights, preserve execution highlights
		// This is tricky - we need to preserve lastHighlightedLine
		for (int i = 0; i < GetLineCount(); i++)
		{
			// Don't clear the current execution line
			if (i != lastHighlightedLine)
			{
				SetLineBackgroundColor(i, new Color(0, 0, 0, 0));
			}
		}
	}

	private void HighlightSyntaxError(int lineNum, Color color)
	{
		if (lineNum >= 0 && lineNum < GetLineCount())
		{
			SetLineBackgroundColor(lineNum, color);
		}
	}

	private void UpdateErrorLabel()
	{
		if (errorLabel == null) return;

		// Clear any old runtime error label
		var runtimeLabel = GetNodeOrNull<Label>("RuntimeErrorLabel");
		
		if (lineErrors.Count == 0)
		{
			errorLabel.Text = "";
			errorLabel.Visible = false;
			
			// Position runtime label if it exists
			if (runtimeLabel != null)
			{
				runtimeLabel.Position = new Vector2(0, this.Size.Y - 20);
			}
			return;
		}

		// Build error message
		var sortedErrors = new List<(int line, string msg)>();
		foreach (var kvp in lineErrors)
		{
			sortedErrors.Add((kvp.Key, kvp.Value));
		}
		sortedErrors.Sort((a, b) => a.line.CompareTo(b.line));

		// Show all problematic line numbers
		string lineNumbers = "Syntax error(s) on line: ";
		for (int i = 0; i < sortedErrors.Count; i++)
		{
			lineNumbers += (sortedErrors[i].line + 1).ToString();
			if (i < sortedErrors.Count - 1)
				lineNumbers += ", ";
		}

		// Show first error message
		string firstError = sortedErrors[0].msg;
		errorLabel.Text = $"{lineNumbers} - {firstError}";
		errorLabel.Visible = true;
		errorLabel.Position = new Vector2(0, this.Size.Y - 20);
		
		// Position runtime label higher if it exists
		if (runtimeLabel != null)
		{
			runtimeLabel.Position = new Vector2(0, this.Size.Y - 40);
		}
	}

	public void TryHighlightingObject()
	{
		GD.Print("CodeEdit: correspondingObject: ", correspondingObject);
		if (correspondingObject != null)
		{
			if (correspondingObject is Scriptable)
			{
				Layer layer = correspondingObject.GetParentLayer();
				if (layer != null)
				{
					layer.HighlightTile(true, correspondingObject.GetCurrPos());
					GD.Print("CodeEdit: Successfully highlighted correspondingObject.");
				}
				
				if (correspondingObject is ConveyorGroup)
				{
					GD.Print("CodeEdit: Detected ConveyorGroup!");
					
					PlaceableObject obj = (PlaceableObject)((ConveyorGroup)correspondingObject).getObjectList()[0];
					
					if (obj == null)
					{
						GD.Print("CodeEdit: ConveyorGroup associated with terminal has no ConveyorObject objects!");
						return;
					}
					
					layer = obj.GetParentLayer();

					if (layer != null)
					{
						layer.HighlightTile(true, obj.GetCurrPos());
						GD.Print("CodeEdit: Successfully highlighted correspondingObject.");
					}
				}
			}
		}
		else
		{
			GD.Print("CodeEdit: correspondingObject is null!");
		}
	}

	public void StopHighlighting()
	{
		// Function to stop highlighting
	}

	public void SetCorrespondingObject(PlaceableObject input)
	{
		correspondingObject = input;
	}

	public string GetCode()
	{
		return Text;
	}
	
	public int getLastHighlighted()
	{
		return lastHighlightedLine;
	}

	// Existing HighlightLine for execution stepping
	public void HighlightLine(int lineNumber, Color color, bool error = false)
	{
		HighlightCurrentLine = false;
		
		// If this is an error highlight from ErrorHandler (runtime error)
		if (error == true)
		{
			GD.PrintErr(lineNumber);
			SetLineBackgroundColor(lineNumber, color);
			lastHighlightedLine = lineNumber;
			QueueRedraw();
			return;
		}

		// Clear previous execution highlight (not syntax errors)
		if (lastHighlightedLine >= 0 && lastHighlightedLine < GetLineCount())
		{
			// Only clear if it's not a syntax error line
			if (!lineErrors.ContainsKey(lastHighlightedLine))
			{
				SetLineBackgroundColor(lastHighlightedLine, new Color(0, 0, 0, 0f));
			}
			else
			{
				// Re-apply syntax error color
				SetLineBackgroundColor(lastHighlightedLine, errorColor);
			}
		}

		int totalLines = GetLineCount();
		if (totalLines == 0)
		{
			return;
		}

		int lineToHighlight = Math.Max(0, lineNumber);

		if (lineToHighlight >= totalLines)
		{
			return;
		}

		// Skip empty lines and labels
		while (lineToHighlight < totalLines)
		{
			string curr = GetLine(lineToHighlight);
			if (string.IsNullOrWhiteSpace(curr) || curr.Trim().EndsWith(":"))
			{
				lineToHighlight++;
				continue;
			}
			string trimmedLine = curr.Trim();
			if (trimmedLine.StartsWith("jmp "))
			{
				string label = trimmedLine.Substring(4).Trim();
				int target = FindNextLineAfterLabel(label);
				if (target >= 0)
				{
					lineToHighlight = target;
					break;
				}
				else
				{
					break;
				}
			}
			break;
		}

		if (lineToHighlight >= totalLines)
		{
			return;
		}

		// Execution highlight (yellow/green) overlays syntax errors
		SetLineBackgroundColor(lineToHighlight, color);
		lastHighlightedLine = lineToHighlight;
		QueueRedraw();
	}

	private int FindNextLineAfterLabel(string labelName)
	{
		int total = GetLineCount();
		if (total == 0)
		{
			return -1;
		}

		for (int i = 0; i < total; i++)
		{
			var line = GetLine(i)?.Trim();
			if (line != null && line.Equals(labelName + ":"))
			{
				for (int j = i + 1; j < total; j++)
				{
					var next = GetLine(j);
					if (!string.IsNullOrWhiteSpace(next) && !next.Trim().EndsWith(":"))
					{
						return j;
					}
				}
				return -1;
			}
		}

		return -1;
	}

	public void ClearAllHighlights()
	{
		for (int i = 0; i < GetLineCount(); i++)
		{
			SetLineBackgroundColor(i, new Color(0, 0, 0, 0f));
		}
		lastHighlightedLine = -1;
		HighlightCurrentLine = true;
		CallDeferred(nameof(ValidateAndHighlight));
	}

	// Error checking API
	public Label GetErrorLabel()
	{
		return errorLabel;
	}

	public bool HasErrors()
	{
		return this.error;
	}

	public Dictionary<int, string> GetLineErrors()
	{
		return new Dictionary<int, string>(lineErrors);
	}

	public void ClearAllErrors()
	{
		ClearSyntaxErrorHighlights();
		lineErrors.Clear();
		this.error = false;
		if (errorLabel != null)
		{
			errorLabel.Text = "";
			errorLabel.Visible = false;
		}
	}
	public void ClearRuntimeErrorLabel()
	{
		var runtimeLabel = GetNodeOrNull<Label>("RuntimeErrorLabel");
		if (runtimeLabel != null)
		{
			runtimeLabel.QueueFree();
			runtimeLabel.Free();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left)
		{
			// Future: add click handling if needed
		}
		base._Input(@event);
	}
}
