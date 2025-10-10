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
		// malformed commands - FTODO in S20 - syntax stuff
		_commandParser.Register(@"^\s*mov\s*$", mArgs =>
		{
			GD.Print("Invalid move command");
			//error
		});

		_commandParser.Register(@"^\s*rot\s*$", rotArgs =>
		{
			GD.Print("Invalid Rotate Command");
			//error

		});

		// Movables
		_commandParser.Register(@"^\s*mov\s+([lrud])\s*$", m =>
		{
			GD.Print($"Command: Move {m.Groups[1].Value}");
			//func call
		});

		_commandParser.Register(@"^\s*drp\s*$", d =>
		{
			GD.Print("Command: Drop");
			//func call
		});

		_commandParser.Register(@"^\s*grb\s*$", g =>
		{
			GD.Print("Command: Grab");
			//func call
		});

		_commandParser.Register(@"^\s*rot\s+([lr])\s*$", r =>
		{
			GD.Print($"Command: Rotate {r.Groups[1].Value}");
			//func call
		});


		// Arithmatic
		_commandParser.Register(@"^\s*add\s+", a =>
		{
			// print
			// check reg exists
			// func call

		});

		_commandParser.Register(@"^\s*sub\s+", s =>
		{
			// print
			// check reg exists
			// func call

		});

		_commandParser.Register(@"^\s*mult\s+", m =>
		{
			// print
			// check reg exists
			// func call

		});

		_commandParser.Register(@"^\s*div\s+", d =>
		{
			// print
			// check reg exists
			// func call

		});

		// Registers/Register interaction


		// control flow

		_commandParser.Register(@"^\s*cmp\s+", d =>
		{
			// print
			// check reg exists
			// other stuff

		});

		// labels need to be stored, and the next line must be advanced
		// jump maps to labels

		// regex for jump + string:
		_commandParser.Register(@"^\s*div\s+", d =>
		{
			// print
			// check label exists
			// func call
			// inside handler - if no match error

		});

		// label
		

		// 
	}


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
