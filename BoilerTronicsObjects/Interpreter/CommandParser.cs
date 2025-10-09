using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace CommandParser
{
	// Delegate is a silly c# thing that lets me treat stuff as objects
	public delegate void CommandHandler(Match match);
	public class CommandParser
	{
		private readonly List<(Regex Pattern, CommandHandler Handler)> _rules = new();
		// register table - flag, genric 1, 2, 3, 
		public void Register(string pattern, CommandHandler handler)
		{
			_rules.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled), handler));
		}

		public bool Process(string input)
		{
			foreach (var (pattern, handler) in _rules)
			{
				var match = pattern.Match(input);
				if (match.Success)
				{
					handler(match);
					GD.Print("TESTTESTSTSTSTSTSTSTSTSTSTSTS");
					return false;
				}
				else
				{
					//cases for error
					return true;
				}
			}
			GD.Print($"Unknown command: {input}");
			return false;
		}
	}
}
