using Godot;
using ParsingTests;

namespace ParsingTests;

public partial class TestRunner : Node
{
	[Export]
	public bool autoRun = true;

	public override void _Ready()
	{
		if (autoRun)
		{
			RunTests();
		}
	}

	public void RunTests()
	{
		var testFramework = new ParserTestFramework();
		testFramework.RunAllTests();
	}

	// Can also be called via input for manual testing
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.F5)
			{
				GD.Print("\n\nRunning tests\n");
				RunTests();
			}
		}
	}
}