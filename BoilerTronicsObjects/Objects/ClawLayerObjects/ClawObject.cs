// TODO: implement in more detail
using Godot;
using System;
using System.Linq;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using Parsing;
using System.Collections;
using System.Collections.Generic;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects
{

	public class ClawObject : PlaceableAnimated, Scriptable, Runnable, QualityObject {
		
		public override int GetCost() { return 100; }
		public new static int GetCostStatic() { return 100; }
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		private PlaceableObject heldObject = null;
		private CodeEdit E;
		private Parser _parser;

		// Used to track the quality of the hook
		private Quality Q;

		public bool moving = false; // used for error checking since the claw can move via multiple methods


		static int layerSourceId = 1;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		/*
		public ClawObject(int OGX, int OGY, int altTitle = 0, Quality Q = Quality.LOW_QUALITY) : base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			this.Q = Q;
			_parser = new Parser(this.Q);
			_parser._Ready();
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
			
			
			// adds two new frames to be used by the Frame system
			// (0) is default visuals
			// (1) is "grab empty"
			AddFrame(new TileTex(new Vector2I(3, 0), 10 + (int) Q));

			// (2) is "grab coal"
			AddFrame(new TileTex(new Vector2I(3, 1), 10 + (int) Q));
			
			// (3) is "grab iron ore"
			AddFrame(new TileTex(new Vector2I(3, 2), 10 + (int) Q));
			
			// (4) is "grab iron bar"
			AddFrame(new TileTex(new Vector2I(3, 3), 10 + (int) Q));

			// (5) is "grab iron plate"
			AddFrame(new TileTex(new Vector2I(3, 4), 10 + (int) Q));

			// (6) is "grab iron rod"
			AddFrame(new TileTex(new Vector2I(3, 5), 10 + (int) Q));

			// (7) is "grab steel bar cool"
			AddFrame(new TileTex(new Vector2I(0, 7), 10 + (int) Q));

			// (8) is "grab steel bar hot"
			AddFrame(new TileTex(new Vector2I(4, 7), 10 + (int) Q));

			// (9) is "grab steel plate cool"
			AddFrame(new TileTex(new Vector2I(0, 8), 10 + (int) Q));

			// (10) is "grab steel plate hot"
			AddFrame(new TileTex(new Vector2I(4, 8), 10 + (int) Q));

			// (11) is "grab steel gear cool"
			AddFrame(new TileTex(new Vector2I(8, 6), 10 + (int) Q));

			// (12) is "grab steel gear hot"
			AddFrame(new TileTex(new Vector2I(4, 6), 10 + (int) Q));
		} // create object

		~ClawObject()
		{
			DestroyTerminal(); // Destroys the terminal for this scriptable
		}
		*/

		// Runnable Interface

		public void Step()
		{
			// Make a call to the parser
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			//E.HighlightLine(E.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
			if (_parser == null)
			{
				GD.PrintErr($"{GetType().Name}: Parser not initialized!");
				return;
			}
			int highlight = _parser.ParseGetLine(this, E, E.Text, manager.currLevel.StepCount, E.Name);

			if (highlight >= 0) {
				E.HighlightLine(highlight, new Color(1, 1, 1, 0.3f));
				if (manager.currLevel.E.HasError()) {
					E.HighlightLine(highlight, new Color(1, 0, 0, 0.3f));
				}
			}

			UpdateRegisterDisplay();
		}

		public void RegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
			if (this.heldObject != null) {
				this.heldObject.ResetPos();
			}

			this.heldObject = null;
			base.ResetPos();
			_parser.Reset(); //disposed object error?
			// heldObject = null;
			E.ClearAllHighlights();
			var existing = E.GetNodeOrNull<Label>("ErrorLabel");

			if (existing != null)
			{
				existing.QueueFree();
			}
			
			// FRAME SYSTEM
			// resets this object's "displayed" visuals by resetting its frame index
			// ResetFrame();
			ResetState();

			UpdateRegisterDisplay();
		}

		// Scriptable interface


		// Methods to deal with terminals
		public CodeEdit GetTerminal()
		{
			return E;
		}
		public void SetParser(Parser parser)
		{
			this._parser = parser;
		}
		
		public Parser GetParser()
		{
			return this._parser;
		}

		private void UpdateRegisterDisplay()
		{
			var manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager?.terminalContainer == null) return;

			// Get the register label from the scene
			var terminalVBox = manager.terminalContainer.GetParent() as VBoxContainer;
			if (terminalVBox == null) return;

			var registerPanel = terminalVBox.GetNodeOrNull<PanelContainer>("RegisterPanel");
			if (registerPanel == null) return;

			var registerLabel = registerPanel.GetNodeOrNull<RegisterLabel>("RegisterLabel");
			if (registerLabel == null) return;

			// Only update if this terminal is currently visible
			var currentTerminal = manager.terminalContainer.GetCurrentTabControl();
			if (currentTerminal == E)
			{
				registerLabel.SetParser(_parser);
			}
		}

		public void CreateTerminal()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Terminals currTerminal = manager.terminalContainer;
			E = currTerminal.AddEditor();
			E.Name = "Claw";
			E.SetCorrespondingObject(this);
			
			// set as active tab
			// currTerminal.SetCurrentTab(currTerminal.GetTabCount() - 1);
			// update terminal highlighting
			// currTerminal.GetCurrentEditor().TerminalSelected();
		}

		public void DestroyTerminal()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.terminalContainer.RemoveEditor(E);
			E = null;
		}

		public void SetScript(string script)
		{
			E.Text = script;
		}

		public string GetScript()
		{
			return E.Text;
		}

		private void throwError(ErrorHandler.ErrorType errorCode)
		{
			BoilerTronicsLevel level = BoilerTronicsGlobalManager.GlobalManager.currLevel;

			Layer parentLayer = this.GetParentLayer();
			Vector2I gridPos = this.GetCurrPos();

			Vector2 localPos = parentLayer.MapToLocal(gridPos);
			Vector2 globalPos = parentLayer.ToGlobal(localPos);

			Vector2 offsetPos = globalPos + new Vector2(16, -16);

			level.E.handleError(errorCode, E, offsetPos);
		}

		// Methods that we can use via commands
		public void Move(string[] args)
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			if (args == null) return; // Error, no command
									  // else if (args[0] != "mov") return; // Not the correct command
			else if (args.Length != 2) return; // Error, invalid args
			else if (this.moving)
			{
				// Error, already moving
				manager.currLevel.HaultObjects();
			}

			Vector2I MoveVector;
			int targetDir;

			switch (args[1])
			{
				case "u":
					MoveVector = new Vector2I(1, -1);
					targetDir = TrackObject.Left;
					break;
				case "d":
					MoveVector = new Vector2I(-1, 1);
					targetDir = TrackObject.Left;
					break;
				case "r":
					MoveVector = new Vector2I(1, 0);
					targetDir = TrackObject.Right;
					break;
				case "l":
					MoveVector = new Vector2I(-1, 0);
					targetDir = TrackObject.Right;
					break;
				default:
					return; // not a valid arg
			}

			Vector2I targetGrid = this.GetPos() + MoveVector;
			//area boundary of tileset
			BoilerTronicsLevel level = BoilerTronicsGlobalManager.GlobalManager.currLevel;
			int maxX = level.x;
			int maxY = level.y;

			//check if coords are out of bounds
			if (targetGrid.X < 0 || targetGrid.Y < 0 || targetGrid.X >= maxX || targetGrid.Y >= maxY)
			{
				throwError(ErrorHandler.ErrorType.ClawOutOfBounds);
				return;
			}

			// Check to make sure we are on a track and the track is the correct orientation
			PlaceableObject currObj = manager.currLevel.rLayer.FindObject(this.GetPos());

			// Error: rail we are on is either none existant or the wrong direction
			if (!(currObj is TrackObject tCurrObj) || tCurrObj.GetDir() != targetDir)
			{
				throwError(ErrorHandler.ErrorType.ClawRail);
				return;
			}

			// Check to make sure we are going to a track and that track is the correct orientation
			PlaceableObject targetObj = manager.currLevel.rLayer.FindObject(this.GetPos() + MoveVector);

			// Error: rail we are going to is either none existant or the wrong direction
			if (!(targetObj is TrackObject tTargetObj) || tTargetObj.GetDir() != targetDir)
			{
				throwError(ErrorHandler.ErrorType.ClawRail);
				return;
			}
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Move);

			MovingObject mObj = new MovingObject(this, MoveVector, manager.currLevel.cLayer, manager.currLevel.DeltaTime);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);

			return;
		}
		
		// Select the correct grab animation
		private void GrabAnim() {
			Vector2I modAtlas = new Vector2I(0, 0);
			int modSourceId = (int) GetQuality();
			
			if (heldObject is CoalObject) {
				TriggerAnimation("grabCoal", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
			} else if (heldObject is IronOreObject) {
				TriggerAnimation("grabIronOre", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
			} else if (heldObject is IronBarObject) {
				TriggerAnimation("grabIronBar", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
			} else if (heldObject is IronPlateObject) {
				TriggerAnimation("grabIronPlate", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
			} else if (heldObject is IronRodObject) {
				TriggerAnimation("grabIronRod", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
			/* TODO: register animations for steel objects
			} else if (heldObject is SteelBarObject sbObj) {
				// TODO: make the animations for these materials
				if (sbObj.hasHeat()) {
					// SetFrameIndex(8);
				} else {
					// SetFrameIndex(7);
				}
			} else if (heldObject is SteelPlateObject spObj) {
				if (spObj.hasHeat()) {
					// SetFrameIndex(10);
				} else {
					// SetFrameIndex(9);
				}
			} else if (heldObject is SteelGearObject sgObj) {
				if (sgObj.hasHeat()) {
					// SetFrameIndex(12);
				} else {
					// SetFrameIndex(11);
				}
			*/
			} else {
				TriggerAnimation("grabEmpty", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
				GD.PrintErr("ClawObject: Err: Grab anim failed to find heldObject, using default empty anim. internal obj: ", heldObject);
			}
		}
		
		// Select the correct drop animation
		private void DropAnim() {
			Vector2I modAtlas = new Vector2I(0, 0);
			int modSourceId = (int) GetQuality();
			
			SetAtlasMod(new Vector2I((int) GetQuality(), 0));
			
			// well i'm silly. in reality, we only need one "drop" animation
			// because the object "drops" the instant the "Drop" call is triggered
			TriggerAnimation("dropEmpty", AnimatingObject.AnimateType.AnimateFull, modAtlas, modSourceId);
		}

		public void Grab(string[] args) {

			GD.Print("ClawObject: Grab func called");
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Grab);

			if (heldObject != null) return; // TODO: make this an error
	
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			
			// Get the factory object below the claw
			PlaceableObject factoryObj = manager.currLevel.fLayer.FindObject(this.GetCurrPos());

			// If there is no factory object, return
			if (factoryObj == null) return;
			// If there is we want to check if it's moveable, if not return
			if (factoryObj is Movable mObj) {
				// If it is, then we want to try to pick it up (or it's contents)
				heldObject = mObj.PickUp();

				if (heldObject is HeatedMaterial hm) hm.setHook(this);

				GD.Print("Pickedup: ", heldObject);

				manager.currLevel.cLayer.UpdateObject(this);
			} else if (factoryObj is BigMovable bmObj) {
				// If it is, then we want to try to pick it up (or it's contents)
				heldObject = bmObj.PickUp(this.GetCurrPos());

				if (heldObject is HeatedMaterial hm) hm.setHook(this);

				manager.currLevel.cLayer.UpdateObject(this);
			}

			// Call to some update frame function that will update based on the held item
			// UpdateFrame();
			
			// only update if object successfully picked up
			if (heldObject != null) {
				GrabAnim();
			}
		}

		// TODO: make these work again
		// The goal of these is to allow for the heated material to change the sprite of the claw when they are done moving
		public void deleteHeld() {
			heldObject = null;
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			// manager.currLevel.cLayer.UpdateObject(this);
			// UpdateFrame();
		}

		public void updateHeld() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			// manager.currLevel.cLayer.UpdateObject(this);
			// UpdateFrame();
		}

		public void Drop(string[] args) {
			GD.Print("ClawObject: Drop func called");
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Drop);
			if (heldObject == null) return; // Not an error ?
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			
			// Get the factory object below the claw
			PlaceableObject factoryObj = manager.currLevel.fLayer.FindObject(this.GetCurrPos());

			Vector2I pos = GetCurrPos();
			heldObject.MoveCurrPos(pos.X, pos.Y);

			if (factoryObj == null) {
				GD.Print("ClawObject: Dropping On Ground\n");
				manager.currLevel.fLayer.AddObject(heldObject);
				heldObject = null;
			} else if (factoryObj is Movable mObj) {
				GD.Print("ClawObject: Dropping Into Factory\n");
				if (mObj.Place(heldObject)) heldObject = null;
			} else if (factoryObj is BigMovable bmObj) {
				GD.Print("ClawObject: Dropping Into Factory\n");
				if (bmObj.Place(heldObject, this.GetCurrPos())) heldObject = null;
			}

			// UpdateFrame();
			
			// only play anim if object is successfully dropped
			if (heldObject == null) {
				DropAnim();
			}
		}

		public void Rotate(string[] args)
		{
			return; // Throw error
		}

		public void Switch(string[] args) {
			return; // Throw error
		}

		// Command methods

		public ClawObject(int OGX, int OGY, int altTitle = 0, Quality Q = Quality.LOW_QUALITY) : base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_parser = new Parser();
			_parser._Ready();
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
			
			// for qualitites
			SetQuality(Q);
			SetAtlasMod(new Vector2I((int) GetQuality(), 0));
			
			// ANIMATIONS //
			
			// anim base for "grab" animations
			List<TileTex> grabAnimBase = new List<TileTex>{
				new TileTex(new Vector2I(3, 0), 10),
				new TileTex(new Vector2I(2, 0), 10),
				new TileTex(new Vector2I(1, 0), 10),
			};
			
			// grab empty
			PlaceableAnimationData grabEmpty = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabEmpty.AddFrame(new TileTex(new Vector2I(0, 0), 10));
			animationData.Add(
				"grabEmpty",
				grabEmpty
			);
			
			// grab coal
			PlaceableAnimationData grabCoal = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabCoal.AddFrame(new TileTex(new Vector2I(0, 1), 10));
			animationData.Add(
				"grabCoal",
				grabCoal
			);
			
			// grab iron ore
			PlaceableAnimationData grabIronOre = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabIronOre.AddFrame(new TileTex(new Vector2I(0, 2), 10));
			animationData.Add(
				"grabIronOre",
				grabIronOre
			);
			
			// grab iron bar
			PlaceableAnimationData grabIronBar = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabIronBar.AddFrame(new TileTex(new Vector2I(0, 3), 10));
			animationData.Add(
				"grabIronBar",
				grabIronBar
			);
			
			// grab iron plate
			PlaceableAnimationData grabIronPlate = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabIronPlate.AddFrame(new TileTex(new Vector2I(0, 4), 10));
			animationData.Add(
				"grabIronPlate",
				grabIronPlate
			);
			
			// grab iron rod
			PlaceableAnimationData grabIronRod = new PlaceableAnimationData(
				grabAnimBase, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			grabIronRod.AddFrame(new TileTex(new Vector2I(0, 5), 10));
			animationData.Add(
				"grabIronRod",
				grabIronRod
			);
			
			
			// drop empty
			List<TileTex> dropEmptyAnim = new List<TileTex>{
				new TileTex(new Vector2I(0, 0), 10),
				new TileTex(new Vector2I(1, 0), 10),
				new TileTex(new Vector2I(2, 0), 10),
				new TileTex(new Vector2I(3, 0), 10),
			};
			PlaceableAnimationData dropEmpty = new PlaceableAnimationData(
				dropEmptyAnim, this, 
				new Vector2I(0, 0), // (0, 0) offset
				0.2);				// each frame is 0.2s long
			animationData.Add(
				"dropEmpty",
				dropEmpty
			);
		} // create object

		~ClawObject()
		{
			DestroyTerminal(); // Destroys the terminal for this scriptable
		}
		
		/*
			README:
			
			If you ever want to modify an object's atlas and source IDs without creating a new object,
			then please DO NOT actually modify the object's atlas/source IDs!
			
			As the save system recognizes/spawns objects based off their source IDs, this really messes
			with the save system if you do so.
			
			Instead, modify the below two functions, as Layers and etc utilize these functions --
			while the PlaceableObjects  still correctly serialize their private internal values.
		*/
		// overrides visuals in accordance to the expected visuals (for quality system)
		// (hacky solution)
		public override int GetSourceID() {
			if (GetStateName() == "default") {
				SetSourceIdMod(0);
			} else {
				SetSourceIdMod((int) GetQuality());
			}
			
			return currData.GetFrame().GetSourceID() + GetSourceIdMod();
		}
		
		public override Vector2I GetAtlasPos() {
			if (GetStateName() == "default") {
				SetAtlasMod(new Vector2I((int) GetQuality(), 0));
			} else {
				SetAtlasMod(new Vector2I(0, 0));
			}
			return currData.GetFrame().GetAtlasPos() + GetAtlasMod();
		}

		// Override 'save' function to also return a script's information
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = base.Save();
			
			// GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			res["terminalCode"] = GetScript();
			res["quality"] = (int) GetQuality();
			return res;
		}

		// Quality Interface
		public void SetQuality(Quality newQuality) {
			Q = newQuality;
			
			// Update sprite?
			SetAtlasMod(new Vector2I((int) GetQuality(), 0));
		}

		public Quality GetQuality() {
			return Q;
		}
		
		// note: we can't use the base "GetTexture()" functionality as this
		// would use the "GetSource()" and "GetAtlasPos()" functions from PlaceableAnimated!
		public override Texture GetTexture()
		{
			// this.GetSourceID();
			// this.GetAtlasPos();
			// GD.Print("ClawObject: GetTexture: sourceId: ", this.GetSourceID(), ", atlasPos: ", this.GetAtlasPos().ToString());
			//return base.GetTexture();
			
			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
			int sourceid = tileSet.GetSourceId(this.GetSourceID());

			TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(this.GetAtlasPos());
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			var texture = new ImageTexture();
			texture.SetImage(imageTexture);

			return texture;
		}
		
		public static Texture GetQualityTexture(Quality Q) {
			// Get the texture based off the quality passed in
			Vector2I atPos = objectAtlasPos + new Vector2I((int) Q, 0);

			var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");

			// int sourceid = tileSet.GetSourceId(ID);
			TileSetAtlasSource tileSetSource = tileSet.GetSource(layerSourceId) as TileSetAtlasSource;

			// get the tile
			var tile = tileSetSource.GetTileTextureRegion(atPos);
			var fullTexture = tileSetSource.Texture.GetImage();
			var imageTexture = fullTexture.GetRegion(tile);
			ImageTexture T = new ImageTexture();

			T.SetImage(imageTexture);

			// Insert in list such that it is in the correct order to draw
			// (Figure this out later)

			return T;
		}
	}
}
