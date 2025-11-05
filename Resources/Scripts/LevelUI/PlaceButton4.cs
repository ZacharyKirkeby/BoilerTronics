using Godot;
using System;

public partial class PlaceButton4 : Button
{
	public override void _Ready()
	{
		Button PlaceButton4 = this;
		PlaceButton4.Pressed += _press_func;
	}

	private void _press_func()
	{
		// DO STUFF HERE
		// GD.Print("test4");
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker.Update(4);
		manager.currSlection = 4;
	}
}
