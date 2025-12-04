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
	private readonly Dictionary<string, int> _registerTTL = new();
	private int REGISTER_DECAY_STEPS = 5;
	private int _stepConsumingInstructionCount = 0;
	private List<int> _sourceLineNumbers = new();
	private List<string> _validLines = new();
	private int _programCounter = 0;
	private bool _programHalted = false;
	private string _currentProgram = "";

	// Current execution context
	private Scriptable scriptObject;
	private CodeEdit currEditor;
	private string editorName;
	private bool _debug = true;
	private bool _decayFlag = true;
	private Quality qualityFlag;

	// Command parser for step-consuming instructions (mov, rot, grb, drp)
	private CommandParser.CommandParser _commandParser = new CommandParser.CommandParser();

	// Store which regex to use based on quality
	private Regex _wrtRegex;
	private Regex _wrtRegisterRegex;
	private Regex _arithRegex;

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

	 //r0–r2 and r0–r3 versions
	[GeneratedRegex(@"^\s*wrt\s+(r[0-2]|cmp)\s+(-?\d+)\s*$")]
	private static partial Regex WrtRegex0_2();
	
	[GeneratedRegex(@"^\s*wrt\s+(r[0-3]|cmp)\s+(-?\d+)\s*$")]
	private static partial Regex WrtRegex0_3();

	// WRT register: r0–r2 and r0–r3 versions
	[GeneratedRegex(@"^\s*wrt\s+(r[0-2]|cmp)\s+(r[0-2]|cmp)\s*$")]
	private static partial Regex WrtRegisterRegex0_2();
	
	[GeneratedRegex(@"^\s*wrt\s+(r[0-3]|cmp)\s+(r[0-3]|cmp)\s*$")]
	private static partial Regex WrtRegisterRegex0_3();

	// Arithmetic commands
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s+(r[0-2]|cmp|-?\d+)\s+(r[0-2]|cmp|-?\d+)\s*$")]
	private static partial Regex ArithRegex0_2();

	// Arithmetic commands - higher quality means more register
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s+(r[0-3]|cmp|-?\d+)\s+(r[0-3]|cmp|-?\d+)\s*$")]
	private static partial Regex ArithRegex0_3();

	// Control commands
	[GeneratedRegex(@"^\s*wait\s*$")]
	private static partial Regex WaitRegex();

	// constructor with a higher quality level
	public Parser(Quality Q)
{
	qualityFlag = Q;
	ProgramValidator.SetQualityLevel((int)Q);
	InitializeRegexes();
}

// default contstructor, quality by default is 0
public Parser()
{
	ProgramValidator.SetQualityLevel(0);
	qualityFlag = 0;//perchance this fixes it?
	InitializeRegexes();
	return;
}

	private void InitializeRegexes()
	{
		// Select which regexes to use based on quality flag
		if ((int)qualityFlag >= 1)
		{
			_wrtRegex = WrtRegex0_3();
			_wrtRegisterRegex = WrtRegisterRegex0_3();
			_arithRegex = ArithRegex0_3();
		}
		else
		{
			_wrtRegex = WrtRegex0_2();
			_wrtRegisterRegex = WrtRegisterRegex0_2();
			_arithRegex = ArithRegex0_2();
		}
	}

	public int getQuality()
	{
		return (int)qualityFlag;
	}
	public override void _Ready()
	{
		InitializeRegisters();
		RegisterCommands();
	}

	private string [] registers;

	// instead of in ready, dedicated function
	private void InitializeRegisters()
	{
		// By default the user has r0, r1, r2, and cmp
		_registers["r0"] = 0;
		_registers["r1"] = 0;
		_registers["r2"] = 0;
		_registers["cmp"] = 0;

		// Da Kill Set
		_registerTTL["r0"] = -1; // TTL-time to live
		_registerTTL["r1"] = -1;
		_registerTTL["r2"] = -1;
		_registerTTL["cmp"] = -1;

		// Quality dictates additional behaviors
		switch ((int)qualityFlag)
		{
			case 0: // default
				REGISTER_DECAY_STEPS = 5;
				this.registers = ["r0", "r1", "r2","cmp"];
				break;
			case 1: // med quality - unlocks r3, register decay takes 7 steps
				REGISTER_DECAY_STEPS = 7;
				_registers["r3"] = 0;
				_registerTTL["r3"] = -1;
				this.registers = ["r0", "r1", "r2", "r3", "cmp"];
				break;
			case 2: // high quality - r3 is a stable register and does not decay
				REGISTER_DECAY_STEPS = 10;
				_registers["r3"] = 0;
				this.registers = ["r0", "r1", "r2", "cmp"];  // stable register being excluded
				break;
			default:
				REGISTER_DECAY_STEPS = 5;
				this.registers = ["r0", "r1", "r2","cmp"];
				break;
		}
	}

	public void ResetRegisters()
	{
		InitializeRegisters();
		_stepConsumingInstructionCount = 0;
	}

	public int GetProgramCounter() => _programCounter;

	public bool IsProgramHalted() => _programHalted;

	public Dictionary<string, int> GetRegisters() => new Dictionary<string, int>(_registers);

	public int GetProgramLength() => _validLines.Count;

	public int GetRegister(string name)
	{
		return _registers.ContainsKey(name) ? _registers[name] : 0;
	}

	public bool LoadProgram(string terminal, bool debug = false)
	{
		_currentProgram = terminal;
		_programCounter = 0;
		_programHalted = false;
		_validLines.Clear();
		_labelMap.Clear();
		_sourceLineNumbers.Clear();

		if (string.IsNullOrWhiteSpace(terminal))
			return false;

		// Use ProgramValidator to preprocess
		var result = ProgramValidator.PreprocessProgram(terminal);
		_sourceLineNumbers.AddRange(result.sourceLineNumbers);

		// Copy results into our readonly collections
		foreach (var kvp in result.labels)
		{
			_labelMap[kvp.Key] = kvp.Value;
		}
		_validLines.AddRange(result.validLines);

		// Handle validation errors
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		if (result.errors.Count > 0)
		{
			foreach (var (lineNum, error) in result.errors)
			{
				GD.PrintErr($"Validation error at line {lineNum}: {error}");
				manager.currLevel.E.OnParserErrorRaised(lineNum, error, editorName);
			}
			return false;
		}

		GD.Print($"Program loaded: {_validLines.Count} instructions, {_labelMap.Count} labels");
		return true;
	}


	// Takes in the terminal text (full text, FTODO can i get just a line?)
	// Takes in the current step, derives line number off that
	public int ParseGetLine(PlaceableObject obj, CodeEdit codeEdit, string terminal, int step, string editor)
	{
		int lastStepConsumingLineNumber = -1;
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
		var result = ProgramValidator.PreprocessProgram(terminal);
		if (result.errors.Count > 0)
		{
			GD.PrintErr("Error Found");
			return -1;
		}

		// Check if program is halted or finished
		if (_programHalted || _validLines.Count == 0)
		{
			return -1;
		}

		// Execute instructions until we hit a step-consuming instruction
		int maxInstructionsPerStep = 1000;
		int instructionsExecuted = 0;

		while (!_programHalted && _programCounter < _validLines.Count && instructionsExecuted < maxInstructionsPerStep)
		{
			string lineToProcess = _validLines[_programCounter];
			int currentPC = _programCounter;

			if (_debug) GD.Print($"Executing line {_programCounter}: {lineToProcess}");

			// Execute instruction and check if it consumes a step
			bool consumesStep = false;
			if (!ExecuteInstruction(lineToProcess, ref _programCounter, out consumesStep))
			{
				if (_debug) GD.PrintErr($"Failed to execute: {lineToProcess}");
				//BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				//manager.currLevel.E.OnParserErrorRaised(currentPC, "Execution error", editorName);
				break;
			}

			// If PC wasn't changed by a jump, increment it
			if (_programCounter == currentPC)
			{
				_programCounter++;
			}

			instructionsExecuted++;

			// If this instruction consumed a step, record its source line and stop
			if (consumesStep)
			{
				// Get the source line number for the instruction that just executed
				if (currentPC >= 0 && currentPC < _sourceLineNumbers.Count)
				{
					lastStepConsumingLineNumber = _sourceLineNumbers[currentPC];
				}

				if (_debug) GD.Print($"Step-consuming instruction at editor line {lastStepConsumingLineNumber}. Stopping.");
				return lastStepConsumingLineNumber;
			}

			// Check if we've reached the end
			if (_programCounter >= _validLines.Count)
			{
				_programHalted = true;
				if (_debug) GD.Print("Program completed execution");
				break;
			}
		}

		if (instructionsExecuted >= maxInstructionsPerStep)
		{
			GD.PrintErr("Maximum instructions per step exceeded - possible infinite loop!");
			_programHalted = true;
		}

		return -1;
	}

	private int GetOperandValue(string operand)
	{
		// Check if it's a register
		if (_registers.ContainsKey(operand))
		{
			return _registers[operand];
		}

		// Otherwise parse as literal
		if (int.TryParse(operand, out int literal))
		{
			return literal;
		}

		// shouldn't reach here if regex is correct but idk 
		GD.PrintErr($"Invalid operand: {operand}");
		return 0;
	}

	private bool ExecuteInstruction(string line, ref int pc, out bool consumesStep)
	{
		consumesStep = false; // Default: instruction doesn't consume a step

		// Handle jumps (unconditional and conditional) - FREE
		if (HandleJumps(line, ref pc))
			return true;

		// Handle wait - NOT FREE
		if (WaitRegex().IsMatch(line))
		{
			if (_debug) GD.Print("Command: Wait");
			ProcessRegisterDecay();
			consumesStep = true;
			return true;
		}

		// Handle write (doesn't consume a step) - FREE
		var wrtMatch = _wrtRegex.Match(line);
		if (wrtMatch.Success)
		{
			string reg = wrtMatch.Groups[1].Value;
			int val = int.Parse(wrtMatch.Groups[2].Value);
			SetRegister(reg, val);
			GD.Print($"Write: {reg} = {val}");
			return true;
		}

		var wrtRegMatch = _wrtRegisterRegex.Match(line);
		if (wrtRegMatch.Success)
		{
			string destReg = wrtRegMatch.Groups[1].Value;
			string srcReg = wrtRegMatch.Groups[2].Value;
			_registers[destReg] = _registers[srcReg];
			GD.Print($"Write: {destReg} = {srcReg} ({_registers[destReg]})");
			return true;
		}

		var arithMatch = _arithRegex.Match(line);
		if (arithMatch.Success)
		{
			string cmd = arithMatch.Groups[1].Value;
			string operand1 = arithMatch.Groups[2].Value;
			string operand2 = arithMatch.Groups[3].Value;

			int val1 = GetOperandValue(operand1);
			int val2 = GetOperandValue(operand2);

			if (val1 == -9999999 || val2 == -9999999)
			{
				GD.PrintErr("Error: operand is NULL due to decay");
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(pc, "Error: operand is NULL due to decay", editorName);
				return false;
			}

			switch (cmd)
			{
				case "add":
					_registers["r0"] = val1 + val2;
					break;
				case "sub":
					_registers["r0"] = val1 - val2;
					break;
				case "mul":
					_registers["r0"] = val1 * val2;
					break;
				case "div":
					if (val2 == 0)
					{
						GD.PrintErr("Divide by zero error");
						//EmitSignal(SignalName.ErrorRaised, pc, "Divide by zero", editorName);
						BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
						manager.currLevel.E.OnParserErrorRaised(pc, "Divide by zero", editorName);
						return false;
					}
					_registers["r0"] = val1 / val2;
					break;
				case "cmp":
					int cmpResult = val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
					_registers["cmp"] = cmpResult;
					break;
			}
			if (_debug) GD.Print($"{cmd} {operand1} {operand2}: r0={_registers["r0"]}, r1={_registers["r1"]}, r2={_registers["r2"]}, cmp={_registers["cmp"]}");
			return true;
		}

		// These DO consume a step
		bool result = _commandParser.Process(line);
		if (result)
		{
			consumesStep = true; // mov, rot, grb, drp all consume steps
			ProcessRegisterDecay();
		}
		return result;
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
		}
		else
		{
			if (_debug) GD.PrintErr($"Undefined label: {label}");
			//EmitSignal(SignalName.ErrorRaised, pc, $"Undefined label: {label}", editorName);
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.E.OnParserErrorRaised(pc, $"Undefined label: {label}", editorName);
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
				//EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(_programCounter, "Invalid command for this object", editorName);
				
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
				//EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(_programCounter, "Invalid command for this object", editorName);
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
				//EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(_programCounter, "Invalid command for this object", editorName);
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
				//EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(_programCounter, "Invalid command for this object", editorName);
			}
		});

		// Switch command - FTODO for someone else
		_commandParser.Register(@"^\s*swt\s*$", m =>
		{
			if (scriptObject != null && scriptObject is SwitchObject)
			{
				GroupCollection groups = m.Groups;
				string[] values = new string[groups.Count];
				for (int i = 0; i < groups.Count; i++)
				{
					values[i] = groups[i].Value;
				}
				scriptObject.Switch(values);
				if (_debug) GD.Print("Switch");
			}
			else
			{
				//EmitSignal(SignalName.ErrorRaised, _programCounter, "Invalid command for this object", editorName);
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				manager.currLevel.E.OnParserErrorRaised(_programCounter, "Invalid command for this object", editorName);
			}
		});
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
	public int GetRegisterTTL(string registerName)
	{
		if (_registerTTL.ContainsKey(registerName))
		{
			return _registerTTL[registerName];
		}
		return -1;
	}

	private void SetRegister(string reg, int value)
	{
		_registers[reg] = value;
		if ((int)qualityFlag != 2 && reg != "r3") _registerTTL[reg] = REGISTER_DECAY_STEPS;

		if (_debug) GD.Print($"Set {reg} = {value}, TTL = {REGISTER_DECAY_STEPS}");
	}

	private void ProcessRegisterDecay()
	{
		_stepConsumingInstructionCount++;
		if (!this._decayFlag)
		{
			return;
		}

		foreach (string reg in registers)
		{
			if (_registerTTL[reg] > 0)
			{
				_registerTTL[reg]--;

				if (_debug) GD.Print($"Decay: {reg} TTL = {_registerTTL[reg]}");

				if (_registerTTL[reg] == 0)
				{
					_registers[reg] = -9999999;
					_registerTTL[reg] = -9999999;

					if (_debug) GD.Print($"Register {reg} decayed to 0");
				}
			}
		}
	}

	public void setDecayFlag(bool input)
	{
		this._decayFlag = input;
	}
}
