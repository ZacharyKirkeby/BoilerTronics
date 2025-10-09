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
		GD.Print("old selection ", manager.currSlection);
		manager.currSlection = 2;
		GD.Print("new selection ", manager.currSlection);
		ObjectPicker objPick = GetNode("%ObjectPicker") as ObjectPicker;
		objPick.Update();
	}
}
