using Godot;
using System;
using System.Collections;				//	ArrayList
using BoilerTronicsObjects.Placeable;	//	PlaceableObject
using BoilerTronicsObjects.Layers;		//	all core layer functionality and etc
using BoilerTronicsObjects.Interfaces;	//	Scriptable interface
using BoilerTronicsObjects.Objects.MovementLayerObjects;	// ConveyorGroup (terminal highlighting, specific exception)

// whole file is arguably a test file
public partial class CodeEdit : Godot.CodeEdit
{
	private int lastHighlightedLine = -1;
	public int currentLine = 0;
	
	
	// important vars for highlighting objects!
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
		
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		// if new terminal is created, then the CurrentEditor() should be this terminal
		// call and update terminal container accordingly
		if (manager.terminalContainer.GetCurrentEditor() == this) {
			manager.terminalContainer.UpdateSelectedTerminal();
		}
	}

	// this is a debug function
	private void OnTextChanged()
	{
		GD.Print($"[{Name}] content changed:\n{Text}");
	}
	
	// custom function, called by Terminals.cs
	// intention is to use this function to check if 'correspondingObject' exists
	// if so, try and "highlight" the object
	// TODO: current Terminals.cs implementation doesn't call this properly on initial level creation
	public void TerminalSelected() {
		GD.Print("terminal selected");
		
		TryHighlightingObject();
	}
	
	public void TryHighlightingObject() {
		GD.Print("CodeEdit: correspondingObject: ", correspondingObject);
		if (correspondingObject != null) {
			
			// Scriptable case
			if (correspondingObject is Scriptable) {
				// highlight corresponding object
				/*
				OLD INEFFICIENT CODE
				only here as a backup/for reference
				
				// determine what layer this object is from
				// TODO: we really should update the PlaceableObject objects to actually hold this data
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				
				// try and find and highlight the specified object
				bool res = HighlightObjectIfValid(correspondingObject, manager.layerClaw);
				if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerFactory); }
				if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerFloor); }
				if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerMovement); }
				if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerRail); }
				
				if (!res) {
					GD.Print("CodeEdit: could not find object on layer to highlight");
				}
				*/
				
				// Extremely simplified method to highlight a tile
				Layer layer = correspondingObject.GetParentLayer();
				if (layer != null) {
					layer.HighlightTile(true, correspondingObject.GetCurrPos());
					GD.Print("CodeEdit: Successfully highlighted correspondingObject.");
				}
				
				if (correspondingObject is ConveyorGroup) {
					// TODO: what if the object is a ConveyorGroup object?
					// need to write exception given that the ConveyorGroup object is extremely distinct
					GD.Print("CodeEdit: Detected ConveyorGroup!");
				}
			}
		} else {
			GD.Print("CodeEdit: correspondingObject is null!");
		}	
	}
	
	// function to stop highlighting and etc
	public void StopHighlighting() {
		// TODO: does not seem to work properly? The below values don't seem to be saved properly
		// Behavior of when a CodeEdit terminal is de-selected is unknown...
		
		// GD.Print("CodeEdit: Trying to stop highlighting");
		// only stop highlighting if needed!
		// if (correspondingObject != null && highlightingObject) {
			// if (highlightedLayer != null) {
				// GD.Print("CodeEdit: Sent stop highlighting request to layer");
				// highlightedLayer.HighlightTile(false, new Vector2I(0, 0));
			// }
		// }
	}
	
	// terrible little helper function
	// returns if the specified layer has the specified object
	// refer to TerminalSelected; this should be deprecated ASAP when good datastructures are adopted and etc
	/*DEPRECATED*/
	private bool ObjectInLayer(PlaceableObject target, Layer layer) {
		ArrayList work = layer.exportObjectList();
		
		// so very efficient (/s)
		// TODO: update PlaceableObject datastruct to hold layer info, edit all instances where a PlaceableObject is constructed accordingly
		foreach (PlaceableObject obj in work) {
			if (obj == target) {
				return true;
			}
		}
		return false;
	}
	
	// yet another terrible little helper function
	// if the object is in the specified layer, then highlight the specified object
	/*DEPRECATED*/
	private bool HighlightObjectIfValid(PlaceableObject target, Layer layer) {
		// if object is not in layer, return
		if (!ObjectInLayer(target, layer)) {return false;}
		
		// else: try and highlight the object!
		// (TODO)
		// GD.Print("found target:", target);
		highlightedLayer = layer;
		layer.HighlightTile(true, target.GetCurrPos());
		
		return true;
	}
	
	
	// sets internal object to point to input
	// mainly just used for the "highlight terminal's corresponding object" functionality
	public void SetCorrespondingObject(PlaceableObject input) {
		correspondingObject = input;
	}

	public string GetCode()
	{
		return Text;
	}
	
	public int getLastHighlighted() {
		return lastHighlightedLine;
	}
	
	//stepping shows current line of execution by highlighting the line in the terminal
	public void HighlightLine(int lineNumber, Color color) {
		HighlightCurrentLine = false;
		if (lastHighlightedLine >= 0 && lastHighlightedLine < GetLineCount()) {
			SetLineBackgroundColor(lastHighlightedLine, new Color(0, 0, 0, 0f));
		}

		int totalLines = GetLineCount();
		if (totalLines == 0) {
			return;
		}

		int lineToHighlight = Math.Max(0, lineNumber);

		//invalid line count
		if (lineToHighlight >= totalLines) {
			return;
		}

		while (lineToHighlight < totalLines) {
			string curr = GetLine(lineToHighlight);
			if (string.IsNullOrWhiteSpace(curr) || curr.Trim().EndsWith(":")) {
				lineToHighlight++;
				continue;
			}
			string trimmedLine = curr.Trim();
			if (trimmedLine.StartsWith("jmp ")) {
				//get label "name"
				string label = trimmedLine.Substring(4).Trim();
				int target = FindNextLineAfterLabel(label);
				if (target >= 0) {
					lineToHighlight = target;
					break;
				}
				else {
					break;
				}
			}
			break;
		}

		// if we've run past the last line while skipping, do nothing
		if (lineToHighlight >= totalLines) {
			return;
		}

		SetLineBackgroundColor(lineToHighlight, color);
		lastHighlightedLine = lineToHighlight;
		QueueRedraw();
	}

	private int FindNextLineAfterLabel(string labelName) {
		int total = GetLineCount();
		if (total == 0) {
			return -1;
		}

		for (int i = 0; i < total; i++) {
			var line = GetLine(i)?.Trim();
			if (line != null && line.Equals(labelName + ":")) {
				//find first line after label
				for (int j = i + 1; j < total; j++) {
					var next = GetLine(j);
					if (!string.IsNullOrWhiteSpace(next) && !next.Trim().EndsWith(":")) {
						return j;
					}
				}
				return -1; //if no line to highlight after
			}
		}

		return -1; //no corresponding label found
	}

	//clears all highlighting for all terminals (when reset button pressed)
	public void ClearAllHighlights() {
		for(int i = 0; i < GetLineCount(); i++) {
			SetLineBackgroundColor(i, new Color(0, 0, 0, 0f));
		}
		lastHighlightedLine = -1;
		HighlightCurrentLine = true;
	}
	
	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left) {
			// TerminalSelected();
		}
		base._Input(@event);
	}
	
}
