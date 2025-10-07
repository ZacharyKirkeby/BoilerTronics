using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommandParser
{
    // Delegate is a silly c# thing that lets me treat stuff as objects
    delegate void CommandHandler(Match match);

    class Program
    {
        static void Main()
        {
            var parser = new CommandParser();

            // Assign regex to set commands

            // Movement Commands
            parser.Register(@"^\s*mov\s+^[lrud]+$\s*$", match => Console.WriteLine($"Move {match.Groups[1].Value}"));
            parser.Register(@"^\s*drp\s*$", _ => Console.WriteLine("Drop command executed"));
            parser.Register(@"^\s*grb\s*$", _ => Console.WriteLine("Grab command executed"));
        }
    }

    class CommandParser
    {
        private readonly List<(Regex Pattern, CommandHandler Handler)> _rules = new();

        public void Register(string pattern, CommandHandler handler)
        {
            _rules.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled), handler));
        }

        public void Process(string input)
        {
            foreach (var (pattern, handler) in _rules)
            {
                var match = pattern.Match(input);
                if (match.Success)
                {
                    handler(match);
                    return;
                }
            }
            Console.WriteLine($"Unknown command: {input}");
        }
    }
}
