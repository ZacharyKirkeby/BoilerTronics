using Godot;
using System;

public partial class PlaceButton3 : Button
{
	public override void _Ready()
	{
		Button PlaceButton3 = this;
		PlaceButton3.Pressed += _press_func;
	}

	private void _press_func()
	{
		// DO STUFF HERE
		// GD.Print("test3");
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.picker.Update(3);
	}
}
