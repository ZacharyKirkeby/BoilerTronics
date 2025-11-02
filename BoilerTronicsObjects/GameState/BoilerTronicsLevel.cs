// This will be the script for the level scene
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Objects.MovementLayerObjects;	// MovementLayer
using Parsing;
using BoilerTronicsObjects.Objects.ClawLayerObjects;

public partial class BoilerTronicsLevel : Node2D
{
	public int x;
	public int y;
	public int StepCount;
	public int cost;
	double deltaTime = 100.0; // time we want it to take to move objects
	TileSet tileset;
	public MovementLayer mLayer;
	public RailLayer rLayer;
	public ClawLayer cLayer;
	public FactoryLayer fLayer;
	public FloorLayer flLayer;
	public ArrayList runnableList = new ArrayList(); // List of runnable Objects
	public ArrayList movingList = new ArrayList(); // List of objects that are currently moving
	public Parser P;
	public ErrorHandler E;
	
	//statistics variables for cutoffs values
	public float ppsCutoff;
	public float cpsCutoff;
	public int rcCutoff;
	
	//statistics variables for solution values
	public float ppsSolution;
	public float cpsSolution;
	public int rcSolution;
	
	public float bestScore = 0;
	
	//levelui reference
	private LevelUi levelUi;
	
	// store all four corners of the placement grid
	private Vector2 c1;
	private Vector2 c2;
	private Vector2 c3;
	private Vector2 c4;
	
	//when solution reached, update solution statistics
	public void UpdateSolutionStats() {
		//TODO: pps based on production/step
		ppsSolution = 0;
		cpsSolution = cost / StepCount;
		//TODO: rc is resources consumed
		rcSolution = 0;
		
		//update leaderboard (min values for the 3 categories)
		//update levelui stats labels
		if(levelUi != null) {
			levelUi.UpdateSolutionStatistics(ppsSolution, cpsSolution, rcSolution);
			float[] grades = levelUi.UpdateSolutionGrading(ppsCutoff, ppsSolution, cpsCutoff, cpsSolution, rcCutoff, rcSolution);
			float solutionScore = grades[0] + grades[1] + grades[2];
			solutionScore /= 3;
			if(solutionScore > bestScore) {
				bestScore = solutionScore;
			}
		}
	}
	
	public void ResetSolutionStats() {
		ppsSolution = 0;
		cpsSolution = 0;
		rcSolution = 0;
		levelUi.SetStatisticDefaults();
	}
	
	public void UpdateCutoffs(float pps, float cps, int rc) {
		ppsCutoff = pps;
		cpsCutoff = cps;
		rcCutoff = rc;
		levelUi.UpdateSolutionCutoffs(pps, cps, rc);
	}
	
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
		GD.Print("Generating Level...");
		
		// Create all of the different layers and read in the corresponding data from the manager
		// Temp, this will be replaced by a read from the global manager's game state
		tileset = GD.Load<TileSet>("res://Resources/objects.tres");
		x = 20;
		y = 20;
		
		// Get manager
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		// For stepping and level interactions
		manager.currLevel = this;
		
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

		// Set Z-index
		manager.layerFloor.ZIndex = 0;
		manager.layerFactory.ZIndex = 1;
		manager.layerClaw.ZIndex = 2;
		manager.layerRail.ZIndex = 3;
		manager.layerMovement.ZIndex = 4;

		// Shift layers
		manager.layerClaw.Position = new Vector2(0, -32);
		manager.layerRail.Position = new Vector2(0, -32);
		manager.layerMovement.Position = new Vector2(0, -32);

		
		if (loadedSave) {
			// attempt to reconstruct level based off the loaded information: update layers
			// TODO: does not properly
			UpdateLayer(manager.layerFloor, manager.GetSaveObjectList("floor"), manager.GetSaveProtectedTiles("floor"));
			UpdateLayer(manager.layerFactory, manager.GetSaveObjectList("factory"), manager.GetSaveProtectedTiles("factory"));
			UpdateLayer(manager.layerClaw, manager.GetSaveObjectList("claw"), manager.GetSaveProtectedTiles("claw"));
			UpdateLayer(manager.layerRail, manager.GetSaveObjectList("rail"), manager.GetSaveProtectedTiles("rail"));
			UpdateLayer(manager.layerMovement, manager.GetSaveObjectList("movement"), manager.GetSaveProtectedTiles("movement"));
			
			// handle ConveyorGroup case
			MovementLayer movement = (MovementLayer) manager.layerMovement;
			foreach (ConveyorGroup obj in movement.ConvGroupList) {
				obj.LoadTerminal();
			}
		}
		
		// store four corners of the floor layer
		c1 = manager.layerFloor.MapToLocal(new Vector2I(0, 0));
		c2 = manager.layerFloor.MapToLocal(new Vector2I(0, y));
		c3 = manager.layerFloor.MapToLocal(new Vector2I(x, y));
		c4 = manager.layerFloor.MapToLocal(new Vector2I(x, 0));
		
		// draw a rectangle representing the boundaries of the placement grid (sorta)
		QueueRedraw();
		
		//get levelui reference to be able to update labels
		levelUi = GetTree().Root.GetNodeOrNull<LevelUi>("Node2D");
		if (levelUi == null) {
			GD.PrintErr("LevelUi not found! Statistics won't update.");
		}

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
			// Free object
			mObj.QueueFree();
		}
		
		StepCount = 0;

		// Empty moving list
		this.movingList.Clear();

		// Clear errors
		E.ClearError();
	}

	/* Handle runnable objects */

	// Steps through all runnables
	public void Step() {
		if (E.HasError()) return; // Can't step if there is an error
		if (movingList.Count != 0) return; // Can't step while stuff is moving
		foreach (PlaceableObject obj in runnableList) {
			if (!(obj is Runnable)) continue; // error here?
			Runnable rObj = (Runnable)obj;
			rObj.Step();
		}
		StepCount++;
		//test for stats
		if(StepCount == 1) {
			UpdateCutoffs(5,6,7);
		}
		else if(StepCount == 5) {
			UpdateSolutionStats();
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

	public void HaultObjects() {
		// Halt all other movement
		foreach (MovingObject obj in movingList) {
			obj.Halt();
		}

	}

	public void MovingCollisionReport(MovingObject mObj) {
		if (mObj == null) return; // We can't report a moving object
		if (!(mObj.obj is PlaceableObject pObj)) return; // We can't report a moving object

		HaultObjects(); // Stop all objects

		Layer parentLayer = pObj.GetParentLayer();
		Vector2I gridPos = pObj.GetCurrPos();

		Vector2 localPos = parentLayer.MapToLocal(gridPos);
		Vector2 globalPos = parentLayer.ToGlobal(localPos);

		Vector2 offsetPos = globalPos + new Vector2(16, -16);

		// Right now we only have collison for claws
		if (pObj is Scriptable sObj) {
			E.handleError(ErrorHandler.ErrorType.ClawRail, sObj.GetTerminal(), offsetPos);
		} else {
			E.handleError(ErrorHandler.ErrorType.ClawCollision, null, offsetPos);
		}
	}
}
