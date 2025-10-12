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
	
	private void CreateMovementLayer() {
		mLayer = new MovementLayer();
		mLayer.TileSet = tileset;
		AddChild(mLayer);
		// Place in elements here!
	}

	private void CreateRailLayer() {
		rLayer = new RailLayer();
		rLayer.TileSet = tileset;
		AddChild(rLayer);
		// Place in elements here!
	}

	private void CreateClawLayer() {
		cLayer = new ClawLayer();
		cLayer.TileSet = tileset;
		AddChild(cLayer);
		// Place in elements here!
	}

	private void CreateFactoryLayer() {
		fLayer = new FactoryLayer();
		fLayer.TileSet = tileset;
		AddChild(fLayer);
		// Place in elements here!
	}

	private void CreateFloorLayer() {
		flLayer = new FloorLayer();
		flLayer.TileSet = tileset;
		AddChild(flLayer);
		// Place in elements here!
	}

	public override void _Ready()
	{
		// Create all of the diffrent layers and read in the corresponding data from the manager
		// Temp, this will be replaced by a read from the global manager's game state
		tileset = GD.Load<TileSet>("res://Resources/objects.tres");
		x = 20;
		y = 20;
		
		CreateFloorLayer();
		CreateFactoryLayer();
		CreateClawLayer();
		CreateRailLayer();
		CreateMovementLayer();

		base._Ready();
	}
}
