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

		// Determine display value
		string displayValue = value.ToString();

		if (_parser != null)
		{
			int ttl = _parser.GetRegisterTTL(regKey);

			// If register had decay and TTL expired, show NULL
			if (ttl == -9999999 && value == 0)
			{
				displayValue = "NULL";
			}

			// Show TTL if still decaying
			if (ttl > 0)
			{
				displayValue += $" ({ttl})";
			}
		}

		return $"{name}: {displayValue}";
	}

}