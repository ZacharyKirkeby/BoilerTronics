using Godot;
using System;

// whole file is arguably a test file
public partial class CodeEdit : Godot.CodeEdit
{
	private int lastHighlightedLine = -1;
	public int currentLine = 0;

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
	
}
