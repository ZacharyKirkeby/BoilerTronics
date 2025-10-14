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
	// for now this is how the labels and jumps will be handled
	private readonly Dictionary<string, int> _labelMap = new();
	// register mapping
	private readonly Dictionary<string, int> _registers = new();

	// for line adjustment for jumps
	
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

		_commandParser.Register(@"^\s*wrt\s+(\w+)\s+(-?\d+)\s*$", m =>
		{
			string reg = m.Groups[1].Value.ToLower();
			int val = int.Parse(m.Groups[2].Value);
			_registers[reg] = val;
			GD.Print($"Write: {reg} = {val}");
		});

		_commandParser.Register(@"^\s*wrt\s+(\w+)\s*$", m =>
			GD.Print($"Malformed Write: Missing value for register {m.Groups[1].Value}"));

		_commandParser.Register(@"^\s*wrt\s*$", _ =>
			GD.Print("Malformed Write: Missing register and value"));

		// MATH OPS + compare
		string[] arith = { "add", "sub", "mult", "div", "cmp" };
		foreach (var cmd in arith)
		{
			// actually correct
			_commandParser.Register($@"^\s*{cmd}\s+(\S+)\s+(\S+)\s*$", m =>
			{
				string reg1 = m.Groups[1].Value.ToLower();
				string reg2 = m.Groups[2].Value.ToLower();

				if (!_registers.ContainsKey(reg1) || !_registers.ContainsKey(reg2))
				{
					GD.Print($"Invalid register(s) in {cmd}: {reg1}, {reg2}");
					return;
				}

				switch (cmd)
				{
					case "add":
						_registers[reg1] += _registers[reg2];
						break;
					case "sub":
						_registers[reg1] -= _registers[reg2];
						break;
					case "mult":
						_registers[reg1] *= _registers[reg2];
						break;
					case "div":
						if (_registers[reg2] == 0)
						{
							GD.Print("Divide by zero error");
							return;
						}
						_registers[reg1] /= _registers[reg2];
						break;
					case "cmp":
						GD.Print($"Compare: {_registers[reg1]} vs {_registers[reg2]}");
						break;
				}

				GD.Print($"Command: {cmd} {reg1} {reg2} => {_registers[reg1]}");
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
		_labelMap.Clear();

		// parses and builds label map - so i can process jumps
		// smh i shouldnt have allowed jumps
		for (int i = 0; i < rawLines.Length; i++)
		{
			string trimmed = rawLines[i].Trim();
			if (string.IsNullOrEmpty(trimmed))
				continue;

			// labels
			if (trimmed.EndsWith(":"))
			{
				string label = trimmed.TrimEnd(':').Trim().ToLower();
				if (!_labelMap.ContainsKey(label))
					_labelMap[label] = validLines.Count;
				continue;
			}

			validLines.Add(trimmed);
		}
		// error handling -> if theres nothing just stop
		if (validLines.Count == 0) { return; }
		CurrLine = step % validLines.Count;

		// update UI about current line
		// TODO - get this to actually work

		// FTODO - REMOVE
		GD.Print(CurrLine);

		if (CurrLine >= validLines.Count)
		{
			GD.Print("Split related issue");
			return;
		}

		string lineToBeProcessed = validLines[CurrLine];
		lineToBeProcessed = lineToBeProcessed.ToLower();

		if (HandleJump(lineToBeProcessed, out int newLine))
		{
			if (newLine >= 0 && newLine < validLines.Count)
			{
				CurrLine = newLine;
				 // update UI for jump target
			}
			else
			{
				GD.Print($"Invalid jump target: {lineToBeProcessed}");
			}
			return;
		}

		if (!_commandParser.Process(lineToBeProcessed))
		{
			GD.Print($"Unknown or malformed command: {lineToBeProcessed}");
		}
	}
	
	private bool HandleJump(string line, out int newLine)
	{
		newLine = -1;

		var match = System.Text.RegularExpressions.Regex.Match(line, @"^\s*jmp\s+(\w+)\s*$");
		if (!match.Success)
			return false;

		string label = match.Groups[1].Value.ToLower();
		if (_labelMap.TryGetValue(label, out int targetIndex))
		{
			newLine = targetIndex;
			GD.Print($"Jumping to label '{label}' at line {targetIndex}");
			return true;
		}

		GD.PrintErr($"Undefined label: {label}");
		return false;
	}
}
