// This will be the script for the level scene
using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Layers;

public partial class BoilerTronicsLevel : Node2D
{
	int x;
	int y;
	TileSet tileset;
	public MovementLayer mLayer;
	public RailLayer rLayer;
	public ClawLayer cLayer;
	public FactoryLayer fLayer;
	public FloorLayer flLayer;
	ArrayList objectList = new ArrayList(); // List of runnable Objects
	
	private void CreateMovementLayer() {
		mLayer = new MovementLayer();
		// mLayer = new MovementLayer(x, y);
		mLayer.TileSet = tileset;
		AddChild(mLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
	}

	private void CreateRailLayer() {
		rLayer = new RailLayer();
		// rLayer = new RailLayer(x, y);
		rLayer.TileSet = tileset;
		AddChild(rLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
	}

	private void CreateClawLayer() {
		cLayer = new ClawLayer();
		// cLayer = new ClawLayer(x, y);
		cLayer.TileSet = tileset;
		AddChild(cLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
	}

	private void CreateFactoryLayer() {
		fLayer = new FactoryLayer();
		// fLayer = new FactoryLayer(x, y);
		fLayer.TileSet = tileset;
		AddChild(fLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
	}

	private void CreateFloorLayer() {
		flLayer = new FloorLayer();
		// flLayer = new FloorLayer(x, y);
		flLayer.TileSet = tileset;
		AddChild(flLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
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
