using Godot;
using System;

public partial class PlaceButton2 : Button
{
	public override void _Ready()
	{
		Button PlaceButton2 = this;
		PlaceButton2.Pressed += _press_func;
	}

	private void _press_func()
	{
		// DO STUFF HERE
		// GD.Print("test2");
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker.Update(2);
		manager.currSlection = 2;
	}
}
