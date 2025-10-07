using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

namespace Parsing;
public partial class Parser : Node
{
    private int maxLineLength;
    private int CurrLine;

    // idk these might be helpful
    

    public string parseTerminal(string line)
    {
        if (line == null)
        {
            return "EmptyInput";
        }

        string inputLine = line.ToLower();

        // regex 





        return "Unrecognized Command";
    }

    // parse ig
    // verify we get input here and #19 is prob bing chilling

    //downward parsing, from simplest to complex

    // grb

    // drp

    // jmp reg1 reg2

    // wrt reg

    // wait

    // rotate: rot ^(l|r)?

    // Move: mov ^(l|r|u|d)?




}
