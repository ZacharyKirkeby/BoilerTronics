using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace CommandParser
{
	public delegate void CommandHandler(Match match);
	public class CommandParser
	{
		private readonly List<(Regex Pattern, CommandHandler Handler)> _rules = new();

		public void Register(string pattern, CommandHandler handler)
		{
			var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			_rules.Add((regex, handler));
		}

		public bool Process(string input)
		{
			foreach (var (pattern, handler) in _rules)
			{
				var match = pattern.Match(input);
				if (match.Success)
				{
					handler(match);
					//GD.Print($"[CommandParser] Matched pattern: {pattern}");
					return true; // Success
				}
			}

			// No pattern matched
			GD.Print($"[CommandParser] Unknown command: {input}");
			return false; // Fail
		}

		public void Clear()
		{
			_rules.Clear();
		}
		public int GetRuleCount()
		{
			return _rules.Count;
		}
	}
}