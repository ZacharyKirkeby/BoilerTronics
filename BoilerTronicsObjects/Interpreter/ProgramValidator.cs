using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace Parsing;
//inshallah my baby is reborn
public static partial class ProgramValidator
{
	// Movement commands
	[GeneratedRegex(@"^\s*mov\s+[lrud]\s*$")]
	private static partial Regex MovValidRegex();
	
	[GeneratedRegex(@"^\s*mov\s*$")]
	private static partial Regex MovEmptyRegex();
	
	[GeneratedRegex(@"^\s*mov\s+\S+")]
	private static partial Regex MovWithArgRegex();
	
	// Rotation commands
	[GeneratedRegex(@"^\s*rot\s+[lr]\s*$")]
	private static partial Regex RotValidRegex();
	
	[GeneratedRegex(@"^\s*rot\s*$")]
	private static partial Regex RotEmptyRegex();
	
	[GeneratedRegex(@"^\s*rot\s+\S+")]
	private static partial Regex RotWithArgRegex();
	
	// Claw commands
	[GeneratedRegex(@"^\s*drp\s*$")]
	private static partial Regex DrpValidRegex();
	
	[GeneratedRegex(@"^\s*drp\s+\S+")]
	private static partial Regex DrpWithArgRegex();
	
	[GeneratedRegex(@"^\s*grb\s*$")]
	private static partial Regex GrbValidRegex();
	
	[GeneratedRegex(@"^\s*grb\s+\S+")]
	private static partial Regex GrbWithArgRegex();
	
	// Write command
	[GeneratedRegex(@"^\s*wrt\s+(r[0-2]|cmp)\s+(-?\d+)\s*$")]
	private static partial Regex WrtValidRegex();
	
	[GeneratedRegex(@"^\s*wrt\s+(r[0-2]|cmp)\s*$")]
	private static partial Regex WrtMissingValueRegex();
	
	[GeneratedRegex(@"^\s*wrt\s*$")]
	private static partial Regex WrtEmptyRegex();
	
	[GeneratedRegex(@"^\s*wrt\s+\S+")]
	private static partial Regex WrtInvalidRegex();
	
	// Arithmetic commands
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s+r[0-2]\s+r[0-2]\s*$")]
	private static partial Regex ArithValidRegex();
	
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s+r[0-2]\s*$")]
	private static partial Regex ArithMissingSecondRegex();
	
	[GeneratedRegex(@"^\s*(add|sub|mul|div|cmp)\s*$")]
	private static partial Regex ArithEmptyRegex();
	
	// Jump commands
	[GeneratedRegex(@"^\s*jmp\s+\w+\s*$")]
	private static partial Regex JmpValidRegex();
	
	[GeneratedRegex(@"^\s*jmp\s*$")]
	private static partial Regex JmpEmptyRegex();
	
	[GeneratedRegex(@"^\s*j(eq|ne|gt|lt|ge|le)\s+\w+\s*$")]
	private static partial Regex ConditionalJmpValidRegex();
	
	[GeneratedRegex(@"^\s*j(eq|ne|gt|lt|ge|le)\s*$")]
	private static partial Regex ConditionalJmpEmptyRegex();
	
	// Wait
	[GeneratedRegex(@"^\s*wait\s*$")]
	private static partial Regex WaitValidRegex();
	
	[GeneratedRegex(@"^\s*wait\s+\S+")]
	private static partial Regex WaitWithArgRegex();
	
	// Labels
	[GeneratedRegex(@"^\s*(\w+):\s*$")]
	private static partial Regex LabelRegex();
	
	// Any non-empty line
	[GeneratedRegex(@"^\s*\S+")]
	private static partial Regex NonEmptyRegex();

    // Validates syntax of entire program
    // Returns list of errors with line numbers
    public static List<(int lineNum, string error)> ValidateProgram(string terminal)
    {
        var errors = new List<(int, string)>();

        if (string.IsNullOrWhiteSpace(terminal))
            return errors;

        string[] rawLines = terminal.Split('\n');
        var tempLabelMap = new Dictionary<string, int>();
        int validLineCount = 0;

        // First pass: check syntax and build label map
        for (int i = 0; i < rawLines.Length; i++)
        {
            string trimmed = rawLines[i].Trim().ToLower();

            if (string.IsNullOrEmpty(trimmed))
                continue;

            // Check for label
            var labelMatch = LabelRegex().Match(trimmed);
            if (labelMatch.Success)
            {
                string label = labelMatch.Groups[1].Value;
                if (string.IsNullOrEmpty(label))
                {
                    errors.Add((i, "Empty label name"));
                    continue;
                }
                if (tempLabelMap.ContainsKey(label))
                {
                    errors.Add((i, $"Duplicate label: {label}"));
                    continue;
                }
                tempLabelMap[label] = validLineCount;
                continue;
            }

            validLineCount++;

            // Validate instruction syntax
            var syntaxError = ValidateInstruction(trimmed);
            if (syntaxError != null)
            {
                errors.Add((i, syntaxError));
            }
        }

        // Second pass: validate jump targets
        for (int i = 0; i < rawLines.Length; i++)
        {
            string trimmed = rawLines[i].Trim().ToLower();

            // Check unconditional jumps
            var jmpMatch = JmpValidRegex().Match(trimmed);
            if (jmpMatch.Success)
            {
                string targetLabel = jmpMatch.Groups[0].Value.Split(' ')[1].Trim();
                if (!tempLabelMap.ContainsKey(targetLabel))
                {
                    errors.Add((i, $"Undefined label: {targetLabel}"));
                }
                continue;
            }

            // Check conditional jumps
            var condJmpMatch = ConditionalJmpValidRegex().Match(trimmed);
            if (condJmpMatch.Success)
            {
                string[] parts = condJmpMatch.Groups[0].Value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string targetLabel = parts[1];
                    if (!tempLabelMap.ContainsKey(targetLabel))
                    {
                        errors.Add((i, $"Undefined label: {targetLabel}"));
                    }
                }
            }
        }

        return errors;
    }

	// Preprocesses program: validates, builds label map, and extracts valid lines
	// Returns labels, validLines, errors
    
	public static (Dictionary<string, int> labels, List<string> validLines, List<(int, string)> errors) 
		PreprocessProgram(string terminal)
	{
		var labels = new Dictionary<string, int>();
		var validLines = new List<string>();
		var errors = new List<(int, string)>();
		
		if (string.IsNullOrWhiteSpace(terminal))
			return (labels, validLines, errors);

		// Run validation first
		errors = ValidateProgram(terminal);

		// Build label map and valid lines
		string[] rawLines = terminal.Split('\n');
		
		for (int i = 0; i < rawLines.Length; i++)
		{
			string trimmed = rawLines[i].Trim().ToLower();
			
			if (string.IsNullOrEmpty(trimmed))
				continue;

			// Handle labels
			var labelMatch = LabelRegex().Match(trimmed);
			if (labelMatch.Success)
			{
				string label = labelMatch.Groups[1].Value;
				if (!string.IsNullOrEmpty(label) && !labels.ContainsKey(label))
				{
					labels[label] = validLines.Count;
				}
				continue;
			}

			// Add valid instruction line
			validLines.Add(trimmed);
		}

		return (labels, validLines, errors);
	}

    // Returns error message if invalid, null if valid
    private static string ValidateInstruction(string line)
    {
        // BEHOLD MY EVIL FUNCTION FULL OF EVIL
        
        // Movement commands
        if (MovValidRegex().IsMatch(line)) return null;
        if (MovEmptyRegex().IsMatch(line)) return "Move missing argument";
        if (MovWithArgRegex().IsMatch(line)) return "Invalid move argument (use l/r/u/d)";

        // Rotation commands
        if (RotValidRegex().IsMatch(line)) return null;
        if (RotEmptyRegex().IsMatch(line)) return "Rotate missing argument";
        if (RotWithArgRegex().IsMatch(line)) return "Invalid rotate argument (use l/r)";

        // Claw commands
        if (DrpValidRegex().IsMatch(line)) return null;
        if (DrpWithArgRegex().IsMatch(line)) return "Drop takes no arguments";

        if (GrbValidRegex().IsMatch(line)) return null;
        if (GrbWithArgRegex().IsMatch(line)) return "Grab takes no arguments";

        // Write command
        if (WrtValidRegex().IsMatch(line)) return null;
        if (WrtMissingValueRegex().IsMatch(line)) return "Write missing value";
        if (WrtEmptyRegex().IsMatch(line)) return "Write missing register and value";
        if (WrtInvalidRegex().IsMatch(line)) return "Invalid register (use r0/r1/r2/cmp)";

        // Arithmetic commands
        if (ArithValidRegex().IsMatch(line)) return null;
        if (ArithMissingSecondRegex().IsMatch(line)) return "Missing second register";
        if (ArithEmptyRegex().IsMatch(line)) return "Missing register arguments";

        // Jump commands
        if (JmpValidRegex().IsMatch(line)) return null;
        if (JmpEmptyRegex().IsMatch(line)) return "Jump missing label";

        if (ConditionalJmpValidRegex().IsMatch(line)) return null;
        if (ConditionalJmpEmptyRegex().IsMatch(line)) return "Conditional jump missing label";

        // Wait
        if (WaitValidRegex().IsMatch(line)) return null;
        if (WaitWithArgRegex().IsMatch(line)) return "Wait takes no arguments";

        // Check if line has any content (not just a label or whitespace)
        if (NonEmptyRegex().IsMatch(line))
            return "Unknown instruction";

        return null;
    }
    
	public static bool IsValid(string terminal)
	{
		var errors = ValidateProgram(terminal);
		return errors.Count == 0;
	}
	public static int GetInstructionCount(string terminal)
	{
		var result = PreprocessProgram(terminal);
		return result.validLines.Count;
	}

	public static List<string> GetLabels(string terminal)
	{
		var result = PreprocessProgram(terminal);
		return new List<string>(result.labels.Keys);
	}

	public static bool HasLabel(string terminal, string labelName)
	{
		var result = PreprocessProgram(terminal);
		return result.labels.ContainsKey(labelName.ToLower());
	}

	public static string GetErrorReport(string terminal)
	{
		var errors = ValidateProgram(terminal);
		
		if (errors.Count == 0)
			return "No errors found";
		
		var report = $"Found {errors.Count} error(s):\n";
		foreach (var (lineNum, error) in errors)
		{
			report += $"  Line {lineNum + 1}: {error}\n";
		}
		
		return report;
	}
}