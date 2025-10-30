using System;
using System.Collections.Generic;
using Godot;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Placeable;
using System.Text.RegularExpressions;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;

namespace Parsing;

public partial class Parser : Node2D
{
	private readonly Dictionary<string, int> _registers = new();
	private readonly Dictionary<string, int> _labelMap = new();
	private List<string> _validLines = new();
	private int _programCounter = 0;
	private bool _programHalted = false;
	private bool _debug = false;
	private string _currentProgram = "";

	// Current execution context
	private Scriptable scriptObject;
	private CodeEdit currEditor;
	private string editorName;

	// Command parser for step-consuming instructions (mov, rot, grb, drp)
	private CommandParser.CommandParser _commandParser = new CommandParser.CommandParser();

	[Signal]
	public delegate void ErrorRaisedEventHandler(int lineNumber, string message, string editorName);

	// None of these consume time steps, hence registered here

	// Jump commands
	[GeneratedRegex(@"^\s*jmp\s+(\w+)\s*$")]
	private static partial Regex JmpRegex();

	[GeneratedRegex(@"^\s*jeq\s+(\w+)\s*$")]
	private static partial Regex JeqRegex();

	[GeneratedRegex(@"^\s*jne\s+(\w+)\s*$")]
	private static partial Regex JneRegex();

	[GeneratedRegex(@"^\s*jgt\s+(\w+)\s*$")]
	private static partial Regex JgtRegex();

	[GeneratedRegex(@"^\s*jlt\s+(\w+)\s*$")]
	private static partial Regex JltRegex();

	[GeneratedRegex(@"^\s*jge\s+(\w+)\s*$")]
	private static partial Regex JgeRegex();

	[GeneratedRegex(@"^\s*jle\s+(\w+)\s*$")]
	private static partial Regex JleRegex();

	[GeneratedRegex(@"^\s*wrt\s+(r[0-2]|cmp)\s+(-?\d+)\s*$")]
	private static partial Regex WrtRegex();

	// Arithmetic commands
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s+(r[0-2])\s+(r[0-2])\s*$")]
	private static partial Regex ArithRegex();

	// Control commands
	[GeneratedRegex(@"^\s*wait\s*$")]
	private static partial Regex WaitRegex();



	public override void _Ready()
	{
		InitializeRegisters();
		RegisterCommands();


		// Below bout to be [Deprecated]
		


		// rot l | r
		_commandParser.Register(@"^\s*rot\s+([lr])\s*$", m =>
		{
			GD.Print($"Command: Rotate {m.Groups[1].Value}");

			//regex collection to string array
			GroupCollection groups = m.Groups;
			string[] values = new string[groups.Count];
			for (int i = 0; i < groups.Count; i++)
			{
				values[i] = groups[i].Value;
			}
			// function call
			if (scriptObject != null & (scriptObject is ConveyorRotatorObject))
			{
				scriptObject.Rotate(values);
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Command for this Object", editorName);
			}
		});

		// rot with the wrong args
		_commandParser.Register(@"^\s*rot\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Rotate argument: {m.Groups[1].Value}");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Rotate Argument", editorName);
		});

		// rot without args
		_commandParser.Register(@"^\s*rot\s*$", m =>
		{
			GD.Print("Malformed Rotate command, missing argument");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Missing Rotate Argument", editorName);
		});


		_commandParser.Register(@"^\s*drp\s*$", d =>
		{
			GD.Print("Command: Drop");

			//regex collection to string array
			GroupCollection groups = d.Groups;
			string[] values = new string[groups.Count];
			for (int i = 0; i < groups.Count; i++)
			{
				values[i] = groups[i].Value;
			}
			// function call
			if (scriptObject != null & (scriptObject is ClawObject))
			{
				scriptObject.Drop(values);
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Command for this Object", editorName);
			}
		});

		// drop with args (bad)
		_commandParser.Register(@"^\s*drp\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Drop argument: {m.Groups[1].Value}");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Drop Argument", editorName);
		});


		_commandParser.Register(@"^\s*grb\s*$", g =>
		{
			GD.Print("Command: Grab");
			//regex collection to string array
			GroupCollection groups = g.Groups;
			string[] values = new string[groups.Count];
			for (int i = 0; i < groups.Count; i++)
			{
				values[i] = groups[i].Value;
			}
			// function call
			if (scriptObject != null & (scriptObject is ClawObject))
			{
				scriptObject.Grab(values);
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Command for this Object", editorName);
			}
		});

		// grab with args (bad)
		_commandParser.Register(@"^\s*grb\s+(\S+)\s*$", m =>
		{
			GD.Print($"Invalid Grab argument: {m.Groups[1].Value}");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Invalid Grab Argument", editorName);
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
		EmitSignal(SignalName.ErrorRaised, CurrLine, "Malformed Write: Missing value", editorName);

		_commandParser.Register(@"^\s*wrt\s*$", _ =>
			GD.Print("Malformed Write: Missing register and value"));
		EmitSignal(SignalName.ErrorRaised, CurrLine, "Malformed Write: Missing Register", editorName);

		// MATH OPS + compare
		string[] arith = { "add", "sub", "mul", "div", "cmp" };
		foreach (var cmd in arith)
		{
			// correct usage
			_commandParser.Register($@"^\s*{cmd}\s+(r[0-2])\s+(r[0-2])\s*$", m =>
			{
				string reg1 = m.Groups[1].Value.ToLower();
				string reg2 = m.Groups[2].Value.ToLower();

				switch (cmd)
				{
					case "add": _registers["r0"] = _registers[reg1] + _registers[reg2]; break;
					case "sub": _registers["r0"] = _registers[reg1] - _registers[reg2]; break;
					case "mul": _registers["r0"] = _registers[reg1] * _registers[reg2]; break;
					case "div":
						if (_registers[reg2] == 0)
						{
							GD.Print("Divide by zero error");
							EmitSignal(SignalName.ErrorRaised, CurrLine, "Divide By Zero Error", editorName);
							return;
						}
						_registers["r0"] = _registers[reg1] / _registers[reg2];
						break;
					case "cmp":
						int cmpResult = _registers[reg1] == _registers[reg2] ? 0 :
										_registers[reg1] < _registers[reg2] ? -1 : 1;
						_registers["cmp"] = cmpResult;
						break;
				}

				GD.Print($"Command: {cmd} {reg1} {reg2} => R0={_registers["r0"]}, R1={_registers["r1"]}, R2={_registers["r2"]}, CMP={_registers["cmp"]}");
			});

			// missing second argument
			_commandParser.Register($@"^\s*{cmd}\s+(r[0-2])\s*$", m =>
			{
				GD.Print($"Invalid {cmd} command, missing second argument");
				EmitSignal(SignalName.ErrorRaised, CurrLine, "Missing Second Argument", editorName);
			});

			// no arguments
			_commandParser.Register($@"^\s*{cmd}\s*$", m =>
			{
				GD.Print($"Malformed {cmd} command, missing arguments");
				EmitSignal(SignalName.ErrorRaised, CurrLine, "Missing Arguments", editorName);
			});
		}

		_commandParser.Register($@"^\s*wait\s*$", m =>
		{
			GD.Print("Command: Wait");
		});

		_commandParser.Register($@"^\s*wait\s+(\S+)\s*$", m =>
		{
			GD.Print("Malformed Wait Unknown Arg");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Malformed Wait", editorName);
		});

		_commandParser.Register($@"^\s*jmp\s+(\S+)\s*$", m =>
		{
			GD.Print("Command: Jump");
		});

		_commandParser.Register($@"^\s*jump\s*$", m =>
		{
			GD.Print("Malformed Jump: Missing Destination");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Malformed Jump: Missing Destination", editorName);
		});

		// placeholder for anything else
		_commandParser.Register(@"^\s*\S+.*$", m =>
		{
			GD.Print($"Unknown command: {m.Value}");
			EmitSignal(SignalName.ErrorRaised, CurrLine, "Unknown Command", editorName);
		});
	}

	// instead of in ready, dedicated function
	private void InitializeRegisters()
	{
		_registers["r0"] = 0;
		_registers["r1"] = 0;
		_registers["r2"] = 0;
		_registers["cmp"] = 0;
	}

	public void ResetRegisters()
	{
		InitializeRegisters();
	}

	public int GetProgramCounter() => _programCounter;

	public bool IsProgramHalted() => _programHalted;
	
	public Dictionary<string, int> GetRegisters() => new Dictionary<string, int>(_registers);
	
	public int GetProgramLength() => _validLines.Count;
	
	public int GetRegister(string name)
	{
		return _registers.ContainsKey(name) ? _registers[name] : 0;
	}






	// Takes in the terminal text (full text, FTODO can i get just a line?)
	// Takes in the current step, derives line number off that
	public void ParseGetLine(PlaceableObject obj, CodeEdit codeEdit, string terminal, int step, string editor)
	{
		if (obj is Scriptable)
		{
			scriptObject = (Scriptable)obj;
			currEditor = codeEdit;
		}
		editorName = editor;
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
		CurrLine = step % validLines.Count; // this is an issue rn

		// update UI about current line
		// TODO - get this to actually work

		string lineToBeProcessed = validLines[CurrLine];
		lineToBeProcessed = lineToBeProcessed.ToLower();

		// since a jump isn't a step consuming task, we pre-handle it
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

		// handle arithmetic here: since it shouldn't consume a step

		// handle -> if end of lines shouldn't continue, unless controlled by JMP

		if (!_commandParser.Process(lineToBeProcessed))
		{
			GD.Print($"Unknown or malformed command: {lineToBeProcessed}");
		}
	}

	// THE LATEST AND GREATEST: More jumping
		private bool HandleJumps(string line, ref int pc)
	{
		// Unconditional jump
		var jmpMatch = JmpRegex().Match(line);
		if (jmpMatch.Success)
		{
			return PerformJump(jmpMatch.Groups[1].Value, ref pc);
		}

		// Conditional jumps based on cmp register
		var jeqMatch = JeqRegex().Match(line);
		if (jeqMatch.Success)
		{
			if (_registers["cmp"] == 0)
				return PerformJump(jeqMatch.Groups[1].Value, ref pc);
			return true; // Condition not met, but valid instruction
		}

		var jneMatch = JneRegex().Match(line);
		if (jneMatch.Success)
		{
			if (_registers["cmp"] != 0)
				return PerformJump(jneMatch.Groups[1].Value, ref pc);
			return true;
		}

		var jgtMatch = JgtRegex().Match(line);
		if (jgtMatch.Success)
		{
			if (_registers["cmp"] > 0)
				return PerformJump(jgtMatch.Groups[1].Value, ref pc);
			return true;
		}

		var jltMatch = JltRegex().Match(line);
		if (jltMatch.Success)
		{
			if (_registers["cmp"] < 0)
				return PerformJump(jltMatch.Groups[1].Value, ref pc);
			return true;
		}

		var jgeMatch = JgeRegex().Match(line);
		if (jgeMatch.Success)
		{
			if (_registers["cmp"] >= 0)
				return PerformJump(jgeMatch.Groups[1].Value, ref pc);
			return true;
		}

		var jleMatch = JleRegex().Match(line);
		if (jleMatch.Success)
		{
			if (_registers["cmp"] <= 0)
				return PerformJump(jleMatch.Groups[1].Value, ref pc);
			return true;
		}
		return false;
	}


	private bool PerformJump(string label, ref int pc)
	{
		if (_labelMap.TryGetValue(label, out int targetIndex))
		{
			pc = targetIndex;
			if (_debug) GD.Print($"Jumping to '{label}' at line {targetIndex}");
			return true;
		} else
        {
            if (_debug) GD.PrintErr($"Undefined label: {label}");
			EmitSignal(SignalName.ErrorRaised, pc, $"Undefined label: {label}", editorName);
			return false;
        }
	}

	[GeneratedRegex(@"^\s*jmp\s+(\w+)\s*$")]
	private static partial Regex MyRegex();

// also instead of in ready
private void RegisterCommands()
	{
		// Movement commands (step-consuming)
		_commandParser.Register(@"^\s*mov\s+([lrud])\s*$", m =>
		{
			if (scriptObject != null && !(scriptObject is ConveyorRotatorObject))
			{
				GroupCollection groups = m.Groups;
				string[] values = new string[groups.Count];
				for (int i = 0; i < groups.Count; i++)
					values[i] = groups[i].Value;
				scriptObject.Move(values);
				GD.Print($"Move {m.Groups[1].Value}");
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
			}
		});

		// Rotation commands (step-consuming)
		_commandParser.Register(@"^\s*rot\s+([lr])\s*$", m =>
		{
			if (scriptObject != null && scriptObject is ConveyorRotatorObject)
			{
				GroupCollection groups = m.Groups;
				string[] values = new string[groups.Count];
				for (int i = 0; i < groups.Count; i++)
					values[i] = groups[i].Value;
				scriptObject.Rotate(values);
				GD.Print($"Rotate {m.Groups[1].Value}");
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
			}
		});

		_commandParser.Register(@"^\s*drp\s*$", m =>
		{
			if (scriptObject != null && scriptObject is ClawObject)
			{
				GroupCollection groups = m.Groups;
				string[] values = new string[groups.Count];
				for (int i = 0; i < groups.Count; i++)
					values[i] = groups[i].Value;
				scriptObject.Drop(values);
				GD.Print("Drop");
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
			}
		});

		// Grab command
		_commandParser.Register(@"^\s*grb\s*$", m =>
		{
			if (scriptObject != null && scriptObject is ClawObject)
			{
				GroupCollection groups = m.Groups;
				string[] values = new string[groups.Count];
				for (int i = 0; i < groups.Count; i++)
					values[i] = groups[i].Value;
				scriptObject.Grab(values);
				GD.Print("Grab");
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
			}
		});
	}

}
