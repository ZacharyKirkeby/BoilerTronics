// TODO: implement in more detail
using Godot;
using System;
using System.Linq;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using Parsing;
using System.Collections;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects
{

	public class ClawObject : PlaceableObject, Scriptable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		private PlaceableObject heldObject = null;
		private CodeEdit E;
		private Parser _parser;

		public bool moving = false; // used for error checking since the claw can move via multiple methods


		static int layerSourceId = 1;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
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
			GD.Print("Claw reset");
			this.heldObject = null;
			base.ResetPos();
			_parser.Reset(); //disposed object error?
			heldObject = null;
			E.ClearAllHighlights();
			var existing = E.GetNodeOrNull<Label>("ErrorLabel");
			if (existing != null)
			{
				existing.QueueFree();
			}
			// Maybe need to make a call to our codeEdit/interrputer?
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
			E = manager.terminalContainer.AddEditor();
			E.Name = "Claw";

			E.SetCorrespondingObject(this);
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

		public void Grab(string[] args) {
			GD.Print("Grab func called");
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Grab);
			if (heldObject != null) return; // TODO: make this an error
			GD.Print("Grabbing object");
	
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			
			// Get the factory object below the claw
			PlaceableObject factoryObj = manager.currLevel.fLayer.FindObject(this.GetCurrPos());
			GD.Print("Factory obj: ", factoryObj);

			// If there is no factory objecy, return
			if (factoryObj == null) return;
			// If there is we want to check if it's moveable, if not return
			if (!(factoryObj is Movable mObj)) return;

			// If it is, then we want to try to pick it up (or it's contents)
			heldObject = mObj.PickUp();
			GD.Print("Pickedup: ", heldObject);
		}

		public void Drop(string[] args) {
			GD.Print("Drop func called");
			BoilerTronicsSoundManager soundManager = BoilerTronicsSoundManager.SoundManager;
			soundManager.PlaySound(SoundType.Drop);
			if (heldObject == null) return; // Not an error ?
			GD.Print("Dropping obj");
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			
			// Get the factory object below the claw
			PlaceableObject factoryObj = manager.currLevel.fLayer.FindObject(this.GetCurrPos());
			GD.Print("Factory obj: ", factoryObj);

			Vector2I pos = GetCurrPos();
			heldObject.MoveCurrPos(pos.X, pos.Y);

			if (factoryObj == null) {
				GD.Print("Placing object on ground");
				manager.currLevel.fLayer.AddObject(heldObject);
				heldObject = null;
			} else if (factoryObj is Movable mObj) {
				GD.Print("Placing object in factory object");
				if (mObj.Place(heldObject)) heldObject = null;
			}
		}

		public void Rotate(string[] args)
		{
			return; // Throw error
		}

		// Command methods
		public ClawObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			_parser = new Parser();
			_parser._Ready();
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		~ClawObject()
		{
			DestroyTerminal(); // Destries the terminal for this scriptable
		}

		// Override 'save' function to also return a script's information
		public override Godot.Collections.Dictionary<string, Variant> Save()
		{
			Godot.Collections.Dictionary<string, Variant> res = base.Save();
			// GD.Print("TODO: override per-object serialization to also include corresponding CodeEdit information");
			res["terminalCode"] = GetScript();
			return res;
		}
	}
}
