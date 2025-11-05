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
	public static BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
	
	public int x;
	public int y;
	public int StepCount;
	public float DeltaTime; // time we want it to take to move objects
	public TileSet tileset;

	public MovementLayer mLayer;
	public RailLayer rLayer;
	public ClawLayer cLayer;
	public FactoryLayer fLayer;
	public FloorLayer flLayer;

	public ArrayList runnableList = new ArrayList(); // List of runnable Objects
	public ArrayList movingList = new ArrayList(); // List of objects that are currently moving

	public Parser P;
	public ErrorHandler E;
	
	// store all four corners of the placement grid
	private Vector2 c1;
	private Vector2 c2;
	private Vector2 c3;
	private Vector2 c4;
	
	// store min, max of X, Y coordinates, based off the dimensions of the level
	public Vector2 minCoords;
	public Vector2 maxCoords;
	
	private BoilerTronicsLevel.GameRunState RunState;

	private float StepDeltaTime = 1.0f; // 1 Second
	private float SlowRunDeltaTime = 1.0f; // 1 Second
	private float FastRunDeltaTime = 0.5f; // Half Second
	private float SubmitStartDeltaTime = 0.5f; // Half Second (this will slowly decrease)
	private float SubmitEndDeltaTime = 0.05f; // .05 Seconds (this will slowly decrease)
	private int SubmitSpeedCahngeStep = 5; // Number of steps between speed changes during submit speed
	private int SubmitSpeedSteps = 10; // Number fo steps between Start and End submit speed
	private int SubmitStartStep = -1; // This will be set when we enter the submit state, this is to allow for a smooth ramp up


	public enum GameRunState {
		Idle = 0,
		Stepping = 1,
		Paused = 2,
		SlowRun = 3,
		FastRun = 4,
		SubmitSpeed = 5,
	}
	
	/* Create layers */

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
	
	// should just automatically fill the "outside" of the layer with some uninteractable floor tiles
	private void CreateFloorFillLayer() {
		FloorFillLayer fillLayer = new FloorFillLayer();
		fillLayer.TileSet = tileset;
		
		// Load boundary data from BoilerTronicsSaveState
		int fillSurround = manager.saveState.boundarySize;
		TileTex floorTex = manager.saveState.boundaryTex;
		fillLayer.GenerateLayer(x, y, fillSurround, floorTex);
		
		// spawn in the tile map
		AddChild(fillLayer);
		
		// translate the top-left edge of this TileMap to the top-left edge of the floor layer
		// (SANITY CHECK)
		Vector2 fill00 = fillLayer.MapToLocal(new Vector2I(0, 0));
		Vector2 floor00 = flLayer.MapToLocal(new Vector2I(0, 0));
		Vector2 moveDif = floor00 - fill00;
		fillLayer.Position -= moveDif;
		// GD.Print("move dif: ", moveDif);
		
		// offset this layer such that this layer properly surrounds the play area
		moveDif = fill00 - flLayer.MapToLocal(new Vector2I(fillSurround, fillSurround));
		fillLayer.Position += moveDif;
		// GD.Print("move dif 2: ", moveDif);
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
			GD.Print("BoilerTronicsLeveL: added protected tile: ", pos);
			bool success = input.SetTileEditable(pos, false);
			// TODO: create error if this coordinate was bad?
		}
	}

	/* init values fpr layer */

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
		bool loadedSave;
		
		if (manager.loadLevelName == "") {
			loadedSave = manager.LoadLevel();
		} else {
			GD.Print("BoilerTronicsLevel: loading specific level");
			// load the specific save and reset the system
			loadedSave = manager.saveState.LoadLevelName(manager, manager.loadLevelName);
			manager.loadLevelName = "";
		}
		
		
		
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
		
		// fills the "outside" of the area with some basic, uninteractable floor tiles
		CreateFloorFillLayer();

		// Set Z-index
		manager.layerFloor.ZIndex = 0;
		manager.layerFactory.ZIndex = 1;
		manager.layerClaw.ZIndex = 2;
		manager.layerRail.ZIndex = 3;
		manager.layerMovement.ZIndex = 4;

		manager.layerFloor.YSortEnabled = true;
		manager.layerFactory.YSortEnabled = true;
		manager.layerClaw.YSortEnabled = true;
		manager.layerRail.YSortEnabled = true;
		manager.layerMovement.YSortEnabled = true;

		// Shift layers
		manager.layerClaw.Position = new Vector2(0, -32);
		manager.layerRail.Position = new Vector2(0, -32);
		manager.layerMovement.Position = new Vector2(0, -32);

		
		if (loadedSave) {
			// attempt to reconstruct level based off the loaded information: update layers
			GD.Print("BoilerTronicsLevel: loading layer: ", "floor");
			UpdateLayer(manager.layerFloor, manager.GetSaveObjectList("floor"), manager.GetSaveProtectedTiles("floor"));
			
			GD.Print("BoilerTronicsLevel: loading layer: ", "factory");
			UpdateLayer(manager.layerFactory, manager.GetSaveObjectList("factory"), manager.GetSaveProtectedTiles("factory"));
			
			GD.Print("BoilerTronicsLevel: loading layer: ", "claw");
			UpdateLayer(manager.layerClaw, manager.GetSaveObjectList("claw"), manager.GetSaveProtectedTiles("claw"));
			
			GD.Print("BoilerTronicsLevel: loading layer: ", "rail");
			UpdateLayer(manager.layerRail, manager.GetSaveObjectList("rail"), manager.GetSaveProtectedTiles("rail"));
			
			GD.Print("BoilerTronicsLevel: loading layer: ", "movement");
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
		
		// update the min, max coordinates
		minCoords = c1;
		maxCoords = c3;

		// Set the run state to Idle
		RunState = BoilerTronicsLevel.GameRunState.Idle;
		DeltaTime = StepDeltaTime;
		
		// draw a rectangle representing the boundaries of the placement grid (sorta)
		QueueRedraw();
		
		/*
		// Prepare parsers for each scriptable element
		foreach (PlaceableObject obj in runnableList) {
			if (!(obj is Runnable)) continue; // error here?
			Runnable rObj = (Runnable)obj;

			if (rObj is Scriptable scriptableObj)
			{
				scriptableObj.SetParser(parser);
				CodeEdit terminal = scriptableObj.GetTerminal();
				if (terminal != null)
				{	
					// scaffoldiong for dynamic errors
					//parser.Connect(Parser.SignalName.ErrorRaised, new Callable(terminal, nameof(terminal.OnParserErrorRaised)));
				
					// Load and validate the program
					if (!string.IsNullOrWhiteSpace(terminal.Text))
					{
						bool error = parser.LoadProgram(terminal.Text);
						// TODO - dynamic error checking terminal.ValidateCode();
						//var errors = terminal.GetValidationErrors();
					}
				}
				else
				{
					GD.PrintErr($"  {obj.GetType().Name} is Scriptable but has no terminal!");
				}
			}
		}	
		base._Ready();
		*/
	}

	/* Draw boarder for layer */
	
	public override void _Draw() {
		
		// Draws the border of the tile map
		// only draw if corners have been determined
		/*
		
		// DISABLED:
		// Surrounding-floor-fill functionality already implemented
		
		if (c1 != null) {
			DrawLine(c1, c2, Colors.Green, 3.0f);
			DrawLine(c2, c3, Colors.Green, 3.0f);
			DrawLine(c3, c4, Colors.Green, 3.0f);
			DrawLine(c4, c1, Colors.Green, 3.0f);
		}
		*/
	}

	/* Reset Layer */

	public void Reset() {
		// Stops moving objects to prevent errors
		HaultObjects();

		// Reset all layers
		mLayer.Reset();
		rLayer.Reset();
		cLayer.Reset();
		fLayer.Reset();
		flLayer.Reset();

		// Loop through moving objects
		foreach (MovingObject mObj in movingList)
		{
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

		foreach (Runnable rObj in runnableList) {
			rObj.Reset();
		}
		
		foreach (Runnable rObj in runnableList)
		{
			rObj.Reset();
		}
		
		StepCount = 0;

		// Empty moving list
		this.movingList.Clear();

		// Clear errors
		E.ClearError();
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		foreach (CodeEdit editor in manager.terminalContainer.GetAllEditors())
		{

			var existing = editor.GetNodeOrNull<Label>("ErrorLabel");
			if (existing != null)
			{
				existing.Free();
			}
		}

		RunState = BoilerTronicsLevel.GameRunState.Idle; // Set to idle
		BoilerTronicsGlobalManager.GlobalManager.unlockTerminals();
		SubmitStartStep = -1;
	}

	/* RunState Management */

	public void Pause() {
		// This will set our state to pause
		RunState = GameRunState.Paused; // Pause, this will stop running
		DeltaTime = StepDeltaTime;
	}

	public void SetStep() {
		// This will set our state to step, this will make sure we can't run after stepping
		RunState = GameRunState.Stepping;
		DeltaTime = StepDeltaTime;
	}

	public void IncRun() {
		switch (RunState) {
			case GameRunState.Idle:
				RunState = GameRunState.SlowRun;
				DeltaTime = SlowRunDeltaTime;
				break;
			case GameRunState.SlowRun:
				RunState = GameRunState.FastRun;
				DeltaTime = FastRunDeltaTime;
				break;
			case GameRunState.FastRun:
				RunState = GameRunState.SubmitSpeed;
				SubmitStartStep = StepCount;
				DeltaTime = SubmitStartDeltaTime;
				break;
		}
	}

	/* Stepping and Running */

	public void Step() {
		if (E.HasError()) {
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Error);
			return; // Can't step if there is an error
		}
		if (movingList.Count != 0) return; // Can't step while stuff is moving
		foreach (PlaceableObject obj in runnableList) {
			if (!(obj is Runnable)) continue; // error here?
			Runnable rObj = (Runnable)obj;
			rObj.Step();
		}
		StepCount++;
	}

	public override void _Process(double delta) {
		// This is where our run will exist to allow for async running
		if (
			(RunState == BoilerTronicsLevel.GameRunState.SlowRun ||
			RunState == BoilerTronicsLevel.GameRunState.FastRun ||
			RunState == BoilerTronicsLevel.GameRunState.SubmitSpeed) &&
			!E.HasError() // Stop running if there's an error
			  )
		{
			BoilerTronicsGlobalManager.GlobalManager.lockTerminals();
			Step(); // Step while we are running

			// if we are on submit speed
			if (RunState == GameRunState.SubmitSpeed && ((StepCount - SubmitStartStep) % SubmitSpeedCahngeStep == 0)) {
				// interpulate between our start and end submit time
				
				// get the percent that we want to interpolate (Current step / Total steps)
				float interpalatePercent = (((float) (StepCount - SubmitStartStep) / (float) SubmitSpeedCahngeStep) / (float) SubmitSpeedSteps);
				// don't continue if we are already at max
				if (interpalatePercent > 1.0f) return;
				// Interpolate between the max and min delta time
				DeltaTime = (SubmitStartDeltaTime * (1.0f - interpalatePercent)) + (SubmitEndDeltaTime * interpalatePercent);
			}
		}
	}

	/* Handle runnable objects */

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

	/* Error Handling */

	// Resume Objects ?? (This could be used in the middle of a step if we pause)

	public void HaultObjects() {
		// Halt all other movement
		foreach (MovingObject obj in movingList) {
			obj.Halt();
		}

	}

	public void MovingCollisionReport(MovingObject mObj, MovingObject other = null) {
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
			CodeEdit terminalMain = sObj.GetTerminal();
			CodeEdit terminalOther = null;
			if (other != null && other.obj is Scriptable otherScript) {
				terminalOther = otherScript.GetTerminal();
			}
			
			E.handleError(ErrorHandler.ErrorType.ClawCollision, sObj.GetTerminal(), offsetPos);

			if (terminalOther != null && terminalOther != terminalMain)
			terminalOther.HighlightLine(terminalOther.getLastHighlighted(), new Color(1, 0, 0, 0.3f));
		} else {
			E.handleError(ErrorHandler.ErrorType.ClawCollision, null, offsetPos);
			GD.Print("actual collision");
		}
	}

	public GameRunState GetGameRunState() {
		return RunState;
	}
}
