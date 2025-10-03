using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

public partial class InterpParser : Node
{
    private int maxLineLength;
    private int CurrLine;

    // idk these might be helpful
    public void Parser()
    {
        this.CurrLine = 0;
        this.maxLineLength = 21;

    }

    public void Parser(int line, int max)
    {
        this.CurrLine = line;
        this.maxLineLength = max;
    }

    // parse ig
    // verify we get input here and #19 is prob bing chilling


    //FTODO in #21- the actual command




}

