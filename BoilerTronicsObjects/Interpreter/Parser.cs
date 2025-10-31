using System;
using System.Collections.Generic;
using Godot;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Placeable;
using System.Text.RegularExpressions;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using System.Diagnostics;

namespace Parsing;

public partial class Parser : Node2D
{
	private readonly Dictionary<string, int> _registers = new();
	private readonly Dictionary<string, int> _labelMap = new();
	private List<string> _validLines = new();
	private int _programCounter = 0;
	private bool _programHalted = false;
	private string _currentProgram = "";

	// Current execution context
	private Scriptable scriptObject;
	private CodeEdit currEditor;
	private string editorName;
	private bool _debug = true;
	private enum qualityFlag;

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
	
	public void LoadProgram(string terminal, bool debug=false)
	{
		_currentProgram = terminal;
		_programCounter = 0;
		_programHalted = false;
		_validLines.Clear();
		_labelMap.Clear();

		if (string.IsNullOrWhiteSpace(terminal))
			return;

		// Use ProgramValidator to preprocess
		var result = ProgramValidator.PreprocessProgram(terminal);
		
		// Copy results into our readonly collections
		foreach (var kvp in result.labels)
		{
			_labelMap[kvp.Key] = kvp.Value;
		}
		_validLines.AddRange(result.validLines);
		
		// Handle validation errors
		if (result.errors.Count > 0)
		{
			foreach (var (lineNum, error) in result.errors)
			{
				GD.PrintErr($"Validation error at line {lineNum}: {error}");
				EmitSignal(SignalName.ErrorRaised, lineNum, error, editorName);
			}
		}
		
		GD.Print($"Program loaded: {_validLines.Count} instructions, {_labelMap.Count} labels");
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

		// Reload program if it changed
		if (terminal != _currentProgram)
		{
			LoadProgram(terminal);
		}

		// Check if program is halted or finished
		if (_programHalted || _validLines.Count == 0)
		{
			return;
		}

		// Check if we've reached the end
		if (_programCounter >= _validLines.Count)
		{
			_programHalted = true;
			if (_debug) GD.Print("Program reached end of execution");
			return;
		}

		string lineToProcess = _validLines[_programCounter];
		int currentPC = _programCounter;

		if (_debug) GD.Print($"Executing line {_programCounter}: {lineToProcess}");

		// Execute instruction
		if (!ExecuteInstruction(lineToProcess, ref _programCounter))
		{
			if (_debug)GD.PrintErr($"Failed to execute: {lineToProcess}");
			EmitSignal(SignalName.ErrorRaised, currentPC, "Execution error", editorName);
		}

		// If PC wasn't changed by a jump, increment it
		if (_programCounter == currentPC)
		{
			_programCounter++;
		}

		// Check if we've now reached the end
		if (_programCounter >= _validLines.Count)
		{
			_programHalted = true;
			if (_debug) GD.Print("Program completed execution");
		}
	}

	private bool ExecuteInstruction(string line, ref int pc)
	{
		// Handle jumps (unconditional and conditional)
		// Jumps don't consume a step, so they modify PC directly
		if (HandleJumps(line, ref pc))
			return true;

		// Handle wait (no-op, consumes a step)
		if (WaitRegex().IsMatch(line))
		{
			GD.Print("Command: Wait");
			return true;
		}

		// Handle write (doesn't consume a step)
		var wrtMatch = WrtRegex().Match(line);
		if (wrtMatch.Success)
		{
			string reg = wrtMatch.Groups[1].Value;
			int val = int.Parse(wrtMatch.Groups[2].Value);
			_registers[reg] = val;
			GD.Print($"Write: {reg} = {val}");
			return true;
		}

		// Handle arithmetic (doesn't consume a step)
		var arithMatch = ArithRegex().Match(line);
		if (arithMatch.Success)
		{
			string cmd = arithMatch.Groups[1].Value;
			string reg1 = arithMatch.Groups[2].Value;
			string reg2 = arithMatch.Groups[3].Value;

			switch (cmd)
			{
				case "add":
					_registers["r0"] = _registers[reg1] + _registers[reg2];
					break;
				case "sub":
					_registers["r0"] = _registers[reg1] - _registers[reg2];
					break;
				case "mul":
					_registers["r0"] = _registers[reg1] * _registers[reg2];
					break;
				case "div":
					if (_registers[reg2] == 0)
					{
						GD.PrintErr("Divide by zero error");
						EmitSignal(SignalName.ErrorRaised, pc, "Divide by zero", editorName);
						return false;
					}
					_registers["r0"] = _registers[reg1] / _registers[reg2];
					break;
				case "cmp":
					int cmpResult = _registers[reg1] == _registers[reg2] ? 0 :
									_registers[reg1] < _registers[reg2] ? -1 : 1;
					_registers["cmp"] = cmpResult;
					break;
			}

			GD.Print($"{cmd}: r0={_registers["r0"]}, r1={_registers["r1"]}, r2={_registers["r2"]}, cmp={_registers["cmp"]}");
			return true;
		}

		// Handle movement/rotation/claw commands via CommandParser
		// These DO consume a step
		return _commandParser.Process(line);
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
				{
					values[i] = groups[i].Value;
				}
				scriptObject.Move(values);
				if (_debug) GD.Print($"Move {m.Groups[1].Value}");
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
				{
					values[i] = groups[i].Value;
				}
				scriptObject.Rotate(values);
				if (_debug) GD.Print($"Rotate {m.Groups[1].Value}");
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
				{
					values[i] = groups[i].Value;
				}
				scriptObject.Drop(values);
				if (_debug) GD.Print("Drop");
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
				{
					values[i] = groups[i].Value;
				}
				scriptObject.Grab(values);
				if (_debug) GD.Print("Grab");
			}
			else
			{
				EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
			}
		});

		// Switch command - FTODO for someone else
	}

	public void ResetProgramCounter()
	{
		_programCounter = 0;
		_programHalted = false;
		_currentProgram = "";
	}
	
	public void Reset()
	{
    	_programCounter = 0;
    	_programHalted = false;
    	_currentProgram = "";
    	_validLines.Clear();
    	_labelMap.Clear();
    	ResetRegisters();
    	if (_debug) GD.Print("Parser reset complete");
	}

}
