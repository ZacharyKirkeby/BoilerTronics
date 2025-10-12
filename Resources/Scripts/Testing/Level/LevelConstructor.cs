using Godot;
using System;
using BoilerTronicsObjects.Layers;

public partial class LevelConstructor : Node2D
{
	int x;
	int y;
	MovementLayer mLayer;
	RailLayer rLayer;
	ClawLayer cLayer;
	FactoryLayer fLayer;
	FloorLayer flLayer;

	TileSet tileset;
	

	public override void _Ready()
	{
		// Create all of the diffrent layers and read in the corresponding data from the manager
		// Temp, this will be replaced by a read from the global manager's game state
		tileset = GD.Load<TileSet>("res://Resources/objects.tres");
		x = 20;
		y = 20;
		
		mLayer = FindChild("MovementLayer") as MovementLayer;
		rLayer = FindChild("RailLayer") as RailLayer;
		cLayer = FindChild("ClawLayer") as ClawLayer;
		fLayer = FindChild("FactoryLayer") as FactoryLayer;
		flLayer = FindChild("FloorLayer") as FloorLayer;

		base._Ready();
	}
}
