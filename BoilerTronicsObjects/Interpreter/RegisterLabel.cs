using Godot;
using System;
using System.Collections.Generic;
using Parsing;

public partial class RegisterLabel : Label
{
	private Parser _parser;
	private Dictionary<string, int> _lastValues = new();

	public override void _Ready()
	{
		// Initialize tracking
		_lastValues["r0"] = 0;
		_lastValues["r1"] = 0;
		_lastValues["r2"] = 0;
		_lastValues["cmp"] = 0;
	}

	public void SetParser(Parser parser)
	{
		_parser = parser;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (_parser == null)
		{
			Text = "R0: 0 | R1: 0 | R2: 0 | CMP: 0";
			return;
		}

		var registers = _parser.GetRegisters();

		// Build display text
		string displayText = "";
		displayText += FormatRegister("R0", registers["r0"]);
		displayText += " | ";
		displayText += FormatRegister("R1", registers["r1"]);
		displayText += " | ";
		displayText += FormatRegister("R2", registers["r2"]);
		displayText += " | ";
		displayText += FormatRegister("CMP", registers["cmp"]);

		Text = displayText;
	}

	private string FormatRegister(string name, int value)
	{
		string regKey = name.ToLower();

		// Track if value changed
		if (_lastValues.ContainsKey(regKey) && _lastValues[regKey] != value)
		{
			// Value changed
		}
		_lastValues[regKey] = value;

		// Format with value and optional TTL indicator
		string formatted = $"{name}: {value}";

		// If register has decay info, show TTL
		if (_parser != null)
		{
			int ttl = _parser.GetRegisterTTL(regKey);
			if (ttl >= 0)
			{
				formatted += $" ({ttl})";
			}
		}

		return formatted;
	}
}