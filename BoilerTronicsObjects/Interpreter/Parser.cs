using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using Godot;

namespace Parsing;
// issue is this being lowercase???
public partial class Parser : Node2D
{
	private int maxLineLength;
	private int CurrLine;
	private CommandParser.CommandParser _commandParser = new CommandParser.CommandParser();


	// command regex lives here 
	public override void _Ready()
	{
		// MOVABLES

		// mov l | r | u | d
		_commandParser.Register(@"^\s*mov\s+([lrud])\s*$", m =>
		{
			GD.Print($"Command: Move {m.Groups[1].Value}");
			//func call
		});

		// invalid mov arg
		_commandParser.Register(@"^\s*mov\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Move argument: {m.Groups[1].Value}");
			// func all
		});

		// empty mov
		_commandParser.Register(@"^\s*mov\s*$", m =>
		{
			GD.Print("Malformed Move command, missing argument");
			// func call
		});

		// rot l | r
		_commandParser.Register(@"^\s*rot\s+([lr])\s*$", m =>
		{
			GD.Print($"Command: Rotate {m.Groups[1].Value}");
			// func call
		});

		// rot with the wrong args
		_commandParser.Register(@"^\s*rot\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Rotate argument: {m.Groups[1].Value}");
			// func call
		});

		// rot without args
		_commandParser.Register(@"^\s*rot\s*$", m =>
		{
			GD.Print("Malformed Rotate command, missing argument");
		});


		_commandParser.Register(@"^\s*drp\s*$", d =>
		{
			GD.Print("Command: Drop");
			//func call
		});

		// drop with args (bad)
		_commandParser.Register(@"^\s*drp\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Drop argument: {m.Groups[1].Value}");
			// func call
		});


		_commandParser.Register(@"^\s*grb\s*$", g =>
		{
			GD.Print("Command: Grab");
			//func call
		});

		// grab with args (bad)
		_commandParser.Register(@"^\s*grb\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Grab argument: {m.Groups[1].Value}");
			// func call
		});

		// MATH OPS + compare
		string[] arith = { "add", "sub", "mult", "div", "cmp" };
		foreach (var cmd in arith)
		{
			// actually correct
			_commandParser.Register($@"^\s*{cmd}\s+(\S+)\s+(\S+)\s*$", m =>
			{
				GD.Print($"Command: {cmd} {m.Groups[1].Value} {m.Groups[2].Value}");
			});

			// missing arg
			_commandParser.Register($@"^\s*{cmd}\s+(\S+)\s*$", m =>
			{
				GD.Print($"Invalid {cmd} command, missing second argument");
			});

			// no args
			_commandParser.Register($@"^\s*{cmd}\s*$", m =>
			{
				GD.Print($"Malformed {cmd} command, missing arguments");
			});
		}

		_commandParser.Register($@"^\s*wait\s*$", m =>
		{
			GD.Print("Command: Wait");
		});

		_commandParser.Register($@"^\s*wait\s+(\S+)\s*$", m =>
		{
			GD.Print("Malformed Wait Unknown Arg");
		});

		_commandParser.Register($@"^\s*jmp\s+(\S+)\s*$", m =>
		{
			GD.Print("Command: Jump");
		});

		_commandParser.Register($@"^\s*jump\s*$", m =>
		{
			GD.Print("Malformed Jump: Missing Destination");
		});

		// placeholder for anything else
		_commandParser.Register(@"^\s*\S+.*$", m =>
		{
			GD.Print($"Unknown command: {m.Value}");
		});
	}

	// Takes in the terminal text (full text, FTODO can i get just a line?)
	// Takes in the current step, derives line number off that
	public void ParseGetLine(string terminal, int step)
	{
		GD.Print("Made it to Parser");
		//CurrLine = line;
		// error handling - i love c#
		if (string.IsNullOrWhiteSpace(terminal))
		{
			return;
		}

		 string[] rawLines = terminal.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		
		List<string> validLines = new();

		foreach (var rawLine in rawLines)
		{
			string trimmed = rawLine.Trim();
			if (string.IsNullOrEmpty(trimmed))
				continue;

			// Skip label definition
			if (trimmed.EndsWith(":"))
			// add to label mapping -> dict with label and the first instruction
				continue;

			validLines.Add(trimmed);
		}

		CurrLine = step % validLines.Count;

		GD.Print(CurrLine);
		if (CurrLine >= validLines.Count)
		{
			GD.Print("Split related issue");
			return;
		}

		string lineToBeProcessed = validLines[CurrLine];
		lineToBeProcessed = lineToBeProcessed.ToLower();

		if (_commandParser.Process(lineToBeProcessed) == false)
		{
			// recurse - idk yet
			GD.Print("No Match");
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
