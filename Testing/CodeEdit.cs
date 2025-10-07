using Godot;
using System;

public partial class CodeEdit : Godot.CodeEdit
{
	public override void _Ready()
	{
		HighlightCurrentLine = true;
		CaretBlink = true;
		TextChanged += OnTextChanged;
	}

	private void OnTextChanged()
	{
		GD.Print($"[{Name}] content changed:\n{Text}");
	}

	public string GetCode()
	{
		return Text;
	}
	
	private int lastHighlightedLine = -1;
	
	//test for stepping to show current line of execution
	public void HighlightLine(int lineNumber) {
		HighlightCurrentLine = false;
		SetLineBackgroundColor(lastHighlightedLine, new Color(0, 0, 0, 0f));
		SetLineBackgroundColor(lineNumber, new Color(1, 1, 0, 0.3f));
		lastHighlightedLine = lineNumber;
	}

	public void ClearAllHighlights() {
		for(int i = 0; i < GetLineCount(); i++) {
			SetLineBackgroundColor(i, new Color(0, 0, 0, 0f));
		}
		lastHighlightedLine = -1;
		HighlightCurrentLine = true;
	}
	
}
