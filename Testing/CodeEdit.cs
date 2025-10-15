using Godot;
using System;
using System.Collections;				//	ArrayList
using BoilerTronicsObjects.Placeable;	//	PlaceableObject
using BoilerTronicsObjects.Layers;		//	all core layer functionality and etc
using BoilerTronicsObjects.Interfaces;	//	Scriptable interface

// whole file is arguably a test file
public partial class CodeEdit : Godot.CodeEdit
{
	private int lastHighlightedLine = -1;
	public int currentLine = 0;
	
	
	// important vars for highlighting objects!
	private PlaceableObject correspondingObject;
	private bool highlightingObject = false;

	public override void _Ready()
	{
		AddToGroup("CodeTerminals");
		HighlightCurrentLine = true;
		CaretBlink = true;
		TextChanged += OnTextChanged;
		currentLine = 0;
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
		
		if (correspondingObject != null && (correspondingObject is Scriptable)) {
			// highlight corresponding object
			
			// determine what layer this object is from
			// TODO: we really should update the PlaceableObject objects to actually hold this data
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			
			// try and find and highlight the specified object
			bool res = HighlightObjectIfValid(correspondingObject, manager.layerClaw);
			if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerFactory); }
			if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerFloor); }
			if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerMovement); }
			if (!res) { HighlightObjectIfValid(correspondingObject, manager.layerRail); }
			
		}
	}
	
	// function to stop highlighting and etc
	public void StopHighlighting() {
		if (correspondingObject != null && highlightingObject) {
			GD.Print("TODO: stop highlighting object");
		}
	}
	
	// terrible little helper function
	// returns if the specified layer has the specified object
	// refer to TerminalSelected; this should be deprecated ASAP when good datastructures are adopted and etc
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
	private bool HighlightObjectIfValid(PlaceableObject target, Layer layer) {
		// if object is not in layer, return
		if (!ObjectInLayer(target, layer)) {return false;}
		
		// else: try and highlight the object!
		// (TODO)
		GD.Print("found target:", target);
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

		int lineToHighlight = lineNumber % totalLines;
		for (int i = 0; i < totalLines; i++)
		{
			string currLineText = GetLine(lineToHighlight);

			//check if valid code line
			if (!string.IsNullOrWhiteSpace(currLineText) && !currLineText.Contains(":")) {
				break;
			}
			lineToHighlight = (lineToHighlight + 1) % totalLines;
		}

		SetLineBackgroundColor(lineToHighlight, color);
		lastHighlightedLine = lineToHighlight;
	}
	
	/*public void HighlightErrorLine(int lineNumber) {
		HighlightCurrentLine = false;
		SetLineBackgroundColor(lastHighlightedLine, new Color(0, 0, 0, 0f));
		SetLineBackgroundColor(lineNumber, new Color(1, 0, 0, 0.3f));
		lastHighlightedLine = lineNumber;
	}*/

	//clears all highlighting for all terminals (when reset button pressed)
	public void ClearAllHighlights() {
		for(int i = 0; i < GetLineCount(); i++) {
			SetLineBackgroundColor(i, new Color(0, 0, 0, 0f));
		}
		lastHighlightedLine = -1;
		HighlightCurrentLine = true;
	}
	
}
