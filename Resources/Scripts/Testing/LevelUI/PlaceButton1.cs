using Godot;
using System;

public partial class PlaceButton1 : Button
{
	public override void _Ready()
	{
		Button PlaceButton1 = this;
		PlaceButton1.Pressed += _press_func;
	}

	private void _press_func()
	{
		// DO STUFF HERE
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		GD.Print("old selection ", manager.currSlection);
		manager.currSlection = 1;
		GD.Print("new selection ", manager.currSlection);
	}
}
