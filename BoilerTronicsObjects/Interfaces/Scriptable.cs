using System;
using System.Collections.Generic;
using Godot;
// this may be changed
public abstract partial class Scriptable : Node
{   
    // Action lets me nest a function call that autofires
    private Dictionary<string, Action<string>> _commandMap;
    public void ExecCommand(string command, string parameter = null)
    {
        _commandMap = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // the instructions/mapping
            { "mov", Move },
            { "rot", Rotate },
            { "drp", _ => Drop() },
            { "grb", _ => Grab() }
        };

        Action<string> action;
        if (_commandMap.TryGetValue(command, out action))
        {
            action(parameter);
        }
        else
        {
            // this literally shouldn't be able to happen
            GD.Print($"Unknown command: {command}");
        }
    }

    // dummy functions idk how ts getting implemented
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
