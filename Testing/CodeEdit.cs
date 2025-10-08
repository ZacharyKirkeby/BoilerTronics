using Godot;
using System;

using Parsing;
public partial class CodeEdit : Godot.CodeEdit
{
	public int currentLine = 0;
	private Parser parser;
	public override void _Ready()
	{
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
		currentLine++;
	}

	public string GetCode()
	{
		return Text;
	}

}
