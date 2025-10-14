// This will be the script for the level scene
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;

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
	
	// store all four corners of the placement grid
	private Vector2 c1;
	private Vector2 c2;
	private Vector2 c3;
	private Vector2 c4;
	
	private Layer CreateMovementLayer() {
		mLayer = new MovementLayer();
		mLayer.RedefineLayer(x, y);
		// mLayer = new MovementLayer(x, y);
		mLayer.TileSet = tileset;
		AddChild(mLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
		// TODO: load from save here
		return mLayer;
	}

	private Layer CreateRailLayer() {
		rLayer = new RailLayer();
		rLayer.RedefineLayer(x, y);
		// rLayer = new RailLayer(x, y);
		rLayer.TileSet = tileset;
		AddChild(rLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
		// TODO: load from save here
		return rLayer;
	}

	private Layer CreateClawLayer() {
		cLayer = new ClawLayer();
		cLayer.RedefineLayer(x, y);
		// cLayer = new ClawLayer(x, y);
		cLayer.TileSet = tileset;
		AddChild(cLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
		// TODO: load from save here
		return cLayer;
	}

	private Layer CreateFactoryLayer() {
		fLayer = new FactoryLayer();
		fLayer.RedefineLayer(x, y);
		// fLayer = new FactoryLayer(x, y);
		fLayer.TileSet = tileset;
		AddChild(fLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
		// TODO: load from save here
		return fLayer;
	}

	private Layer CreateFloorLayer() {
		flLayer = new FloorLayer();
		flLayer.RedefineLayer(x, y);
		// flLayer = new FloorLayer(x, y);
		flLayer.TileSet = tileset;
		AddChild(flLayer);
		// Place in elements here!
		// This will be gotten from the save state in the global manager
		// TODO: load from save here
		return flLayer;
	}
	
	// Given a target layer, a list of Placeables, and a Vector2I array of protected tiles, update the layer!
	// Should only be used when loading info
	private void UpdateLayer(Layer input, ArrayList objects, Vector2I[] protectedTiles) {
		// place all objects
		foreach(PlaceableObject obj in objects)
		{
			input.AddObject(obj);
		}
		
		// enable protected tiles
		foreach(Vector2I pos in protectedTiles)
		{
			bool success = input.SetTileEditable(pos, true);
			// TODO: create error if this coordinate was bad?
		}
	}

	public override void _Ready()
	{
		// Create all of the diffrent layers and read in the corresponding data from the manager
		// Temp, this will be replaced by a read from the global manager's game state
		tileset = GD.Load<TileSet>("res://Resources/objects.tres");
		x = 20;
		y = 20;
		
		// Get manager
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// if successful, then generate level
		// if not, then ignore and make a new save (kinda)
		// TODO: for specific levels, load specific saves corresponding to what the level should be at a baseline!
		bool loadedSave = manager.LoadLevel();
		
		if (loadedSave) {
			// reconstruct level based off the information loaded: load metadata
			// assume that the save state already knows the current level state and etc!
			Vector2I dim = manager.GetLevelDimensions();
			x = dim.X;
			y = dim.Y;
		}
		
		// update manager to hold current level's dimensions (to be used w save system)
		manager.SetLevelDimensions(new Vector2I(x, y));
		
		manager.layerFloor = CreateFloorLayer();
		manager.layerFactory = CreateFactoryLayer();
		manager.layerClaw = CreateClawLayer();
		manager.layerRail = CreateRailLayer();
		manager.layerMovement = CreateMovementLayer();
		
		if (loadedSave) {
			// attempt to reconstruct level based off the loaded information: update layers
			// TODO: does not properly
			UpdateLayer(manager.layerFloor, manager.GetSaveObjectList("floor"), manager.GetSaveProtectedTiles("floor"));
			UpdateLayer(manager.layerFactory, manager.GetSaveObjectList("factory"), manager.GetSaveProtectedTiles("factory"));
			UpdateLayer(manager.layerClaw, manager.GetSaveObjectList("claw"), manager.GetSaveProtectedTiles("claw"));
			UpdateLayer(manager.layerRail, manager.GetSaveObjectList("rail"), manager.GetSaveProtectedTiles("rail"));
			UpdateLayer(manager.layerMovement, manager.GetSaveObjectList("movement"), manager.GetSaveProtectedTiles("movement"));
		}
		
		// store four corners of the floor layer
		c1 = manager.layerFloor.MapToLocal(new Vector2I(0, 0));
		c2 = manager.layerFloor.MapToLocal(new Vector2I(0, y));
		c3 = manager.layerFloor.MapToLocal(new Vector2I(x, y));
		c4 = manager.layerFloor.MapToLocal(new Vector2I(x, 0));
		
		// draw a rectangle representing the boundaries of the placement grid (sorta)
		QueueRedraw();

		base._Ready();
	}
	
	public override void _Draw() {
		
		// Draws the border of the tile map
		// only draw if corners have been determined
		if (c1 != null) {
			DrawLine(c1, c2, Colors.Green, 3.0f);
			DrawLine(c2, c3, Colors.Green, 3.0f);
			DrawLine(c3, c4, Colors.Green, 3.0f);
			DrawLine(c4, c1, Colors.Green, 3.0f);
		}
	}
}
