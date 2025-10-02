using Godot;
using System;

public partial class TextEditor2 : CodeEdit {
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

}
