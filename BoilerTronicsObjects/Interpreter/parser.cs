using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

namespace Parsing;

using CommandParser;
public partial class Parser : Node
{
	private int maxLineLength;
	private int CurrLine;
	private CommandParser _commandParser = new CommandParser();

	// command regex lives here 
	public override void _Ready()
	{
        // malformed commands - FTODO in S20 - syntax stuff
        _commandParser.Register(@"^\s*mov\s*$", _ => GD.Print("Invalid move command"));
        _commandParser.Register(@"^\s*rot\s*$", _ => GD.Print("Invalid Rotate command"));

		// Movables
        _commandParser.Register(@"^\s*mov\s+([lrud])\s*$", m => GD.Print($"Command: Move {m.Groups[1].Value}"));
		_commandParser.Register(@"^\s*drp\s*$", _ => GD.Print("Command: Drop"));
		_commandParser.Register(@"^\s*grb\s*$", _ => GD.Print("Command: Grab"));
		_commandParser.Register(@"^\s*rot\s+([lr])\s*$", m => GD.Print($"Command: Rotate {m.Groups[1].Value}"));


		// Arithmatic

		// Registers/Register interaction
		

		// control flow
	}


	public void ParseGetLine(string terminal, int line)
	{
		GD.Print("Made it to Parser");
		CurrLine = line;
		// error handling - i love c#
		if (string.IsNullOrWhiteSpace(terminal) || line < 0)
		{
			return;
		}

		string[] lines = terminal.Split('\n');

		GD.Print(line, lines.Length.ToString());
		if (line >= lines.Length)
		{
			GD.Print("Split related issue");
			GD.Print(line);
			GD.Print(lines);
			return;
		}

		string lineToBeProcessed = lines[line];
		lineToBeProcessed.ToLower();

		if (_commandParser.Process(lineToBeProcessed) == true)
		{
			// recurse - idk yet
		}
		;

		//call parse

	}

	// parse ig
	// verify we get input here and #19 is prob bing chilling

	//downward parsing, from simplest to complex

	// grb

	// drp

	// jmp reg1 reg2

	// wrt reg

	// wait

	// rotate: rot ^(l|r)?

	// Move: mov ^(l|r|u|d)?




}
