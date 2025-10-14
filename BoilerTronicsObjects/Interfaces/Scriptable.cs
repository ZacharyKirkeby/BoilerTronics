using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Scriptable : Node
{
    private readonly Dictionary<string, Action<string, string>> _commandMap;

    public Scriptable()
    {
        _commandMap = new Dictionary<string, Action<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "mov", (a, _) => Move(a) },
            { "rot", (a, _) => Rotate(a) },
            { "wrt", (a, _) => Write(a) },

            { "drp", (_, _) => Drop() },
            { "grb", (_, _) => Grab() },
            { "wait", (_, _) => Wait() },

            { "add", (a, b) => Add(a, b) },
            { "sub", (a, b) => Subtract(a, b) },
            { "mul", (a, b) => Multiply(a, b) },
            { "div", (a, b) => Divide(a, b) },
            { "cmp", (a, b) => Compare(a, b) },
        };
    }

    public void ExecCommand(string command, string param1 = null, string param2 = null)
    {
        if (_commandMap.TryGetValue(command, out var action))
        {
            action(param1, param2);
        }
        else
        {
            GD.PrintErr($"Unknown command: {command}");
        }
    }

    // Abstract operations
    protected abstract void Move(string direction);
    protected abstract void Rotate(string direction);
    protected abstract void Drop();
    protected abstract void Grab();
    protected abstract void Wait();
    protected abstract void Write(string register);
    protected abstract void Add(string reg1, string reg2);
    protected abstract void Subtract(string reg1, string reg2);
    protected abstract void Multiply(string reg1, string reg2);
    protected abstract void Divide(string reg1, string reg2);
    protected abstract void Compare(string reg1, string reg2);
}
