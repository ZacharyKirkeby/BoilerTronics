// This will be the script for the level scene
using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

public partial class BoilerTronicsLevel : Node2D
{
	int x;
	int y;
	double deltaTime = 100.0; // time we want it to take to move objects
	TileSet tileset;
	public MovementLayer mLayer;
	public RailLayer rLayer;
	public ClawLayer cLayer;
	public FactoryLayer fLayer;
	public FloorLayer flLayer;
	ArrayList runnableList = new ArrayList(); // List of runnable Objects
	ArrayList movingList = new ArrayList(); // List of objects that are currently moving
	
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

	public void Reset() {
		// Reset all layers
		mLayer.Reset();
		rLayer.Reset();
		cLayer.Reset();
		fLayer.Reset();
		flLayer.Reset();
		
		// Loop through moving objects
		foreach (MovingObject mObj in movingList) {
			// Get object and layer
			PlaceableObject obj = mObj.obj;
			Layer layer = mObj.layer;
			// Reset object
			obj.ResetPos();
			// Add back to it's layer
			layer.AddObject(obj);
			// Remove from movingList
			movingList.Remove(mObj);
			// Free object
			mObj.QueueFree();
		}
	}

	/* Handle runnable objects */

	// Steps through all runnables
	public void Step() {
		foreach (PlaceableObject obj in runnableList) {
			if (!(obj is Runnable)) continue; // error here?
			Runnable rObj = (Runnable)obj;
			rObj.Step();
		}
	}

	public void RegisterRunnable(PlaceableObject obj) {
		// Add error checks later
		if (!(obj is Runnable)) return;
		if (runnableList.Contains(obj)) return;
		runnableList.Add(obj);
	}

	public void UnRegisterRunnable(PlaceableObject obj) {
		// Add error checks later
		if (!(obj is Runnable)) return;
		if (!(runnableList.Contains(obj))) return;
		runnableList.Remove(obj);
	}
	
	/* Handle Moving Objects */
	public void RegisterMoving(MovingObject mObj) {
		movingList.Add(mObj);
	}

	public void UnRegisterMoving(MovingObject mObj) {
		movingList.Remove(mObj);
	}

	public void MovingCollisionReport(MovingObject mObj) {
		// This will cause an error

		// Halt all other movement
		foreach (MovingObject obj in movingList) {
			obj.Halt();
		}
	}
}
