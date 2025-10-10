using Godot;
using System;

using Parsing;
public partial class CodeEdit : Godot.CodeEdit
{
<<<<<<< HEAD
	public int currentLine = 0;
	private Parser parser;
=======
	private int lastHighlightedLine = -1;
	
>>>>>>> c6110bcf8b294ac07dcbb1242dac2dd6158c548a
	public override void _Ready()
	{
		AddToGroup("CodeTerminals");
		HighlightCurrentLine = true;
		CaretBlink = true;
		TextChanged += OnTextChanged;
		currentLine = 0;
		parser = GetNode<Parser>("/root/Node2D/MainVBox/TerminalLevelSplit/Parser");

	}

	private void OnTextChanged()
	{
		GD.Print($"[{Name}] content changed:\n{Text}");
		parser.ParseGetLine(Text, currentLine);
		GD.Print("Called Parser");
		GD.Print(currentLine.ToString());
	}

	public string GetCode()
	{
		return Text;
	}
<<<<<<< HEAD

=======
	
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
	
>>>>>>> c6110bcf8b294ac07dcbb1242dac2dd6158c548a
}
