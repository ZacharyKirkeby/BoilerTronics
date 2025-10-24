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

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class ClawObject : PlaceableObject, Scriptable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		private PlaceableObject heldObject = null;
		private CodeEdit E;

		public bool moving = false; // used for error checking since the claw can move via multiple methods


		static int layerSourceId = 1;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Runnable Interface
		public void Step() {
			// Make a call to the parser
			GD.Print("Claw step");
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.P.ParseGetLine(this, E, E.Text, manager.currLevel.StepCount, E.Name);
			E.HighlightLine(E.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
			base.ResetPos();
			heldObject = null;
			// Maybe need to make a call to our codeEdit/interrputer?
		}

		// Scriptable interface


		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Claw";
			
			E.SetCorrespondingObject(this);
		}

		public void DestroyTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.terminalContainer.RemoveEditor(E);
			E = null;
		}

		public void SetScript(string script) {
			E.Text = script;
		}

		public string GetScript() {
			return E.Text;
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			if (args == null) return; // Error, no command
			// else if (args[0] != "mov") return; // Not the correct command
			else if (args.Length != 2) return; // Error, invalid args
			else if (this.moving) {
				// Error, already moving
				manager.currLevel.MovingCollisionReport(null);
			}

			Vector2I MoveVector;
			int targetDir;

			switch (args[1]) {
				case "u":
					MoveVector = new Vector2I(1, -1);
					targetDir = TrackObject.Left;
					GD.Print("up");
					break;
				case "d":
					MoveVector = new Vector2I(-1, 1);
					targetDir = TrackObject.Left;
					GD.Print("down");
					break;
				case "r":
					MoveVector = new Vector2I(1, 0);
					targetDir = TrackObject.Right;
					GD.Print("right");
					break;
				case "l":
					MoveVector = new Vector2I(-1, 0);
					targetDir = TrackObject.Right;
					GD.Print("left");
					break;
				default:
					GD.Print("invaid");
					return; // not a valid arg
			}

			//be able to refer to functions in LevelUi
			var ui = manager.GetTree().CurrentScene as LevelUi;
			
			//tile dimension
			int tileSize = 16;
			
			Vector2I targetGrid = this.GetPos() + MoveVector;
			//area boundary of tileset
			var level = BoilerTronicsGlobalManager.GlobalManager.currLevel as BoilerTronicsLevel;
			int maxX = level.x;
			int maxY = level.y;
			
			GD.Print(targetGrid.X);
			GD.Print(targetGrid.Y);
			
			//check if coords are out of bounds
			if (targetGrid.X < 0 || targetGrid.Y < 0 || targetGrid.X >= maxX || targetGrid.Y >= maxY) {
				ui.setError(1, E.Name);
				Vector2I gridPos = this.GetCurrPos();
				Vector2I pixelPos = new Vector2I((gridPos.X) * tileSize, (gridPos.Y) * tileSize);
				Vector2I pixelPosWithOffset = new Vector2I((gridPos.X) * tileSize, (gridPos.Y) * tileSize);
				ui.setErrorCoords(pixelPosWithOffset);
				return;
			}

			// Check to make sure we are on a track and the track is the correct orientation
			PlaceableObject currObj = manager.currLevel.rLayer.FindObject(this.GetPos());
			// Error: rail we are on is either none existant or the wrong direction
			if (!(currObj is TrackObject tCurrObj) || tCurrObj.GetDir() != targetDir) {
				ui.setError(0, E.Name);
				Vector2I gridPos = this.GetCurrPos();
				Vector2I pixelPos = new Vector2I((gridPos.X) * tileSize, (gridPos.Y) * tileSize);
				Vector2I pixelPosWithOffset = new Vector2I((gridPos.X + 2) * tileSize, (gridPos.Y - 1) * tileSize);
				ui.setErrorCoords(pixelPosWithOffset);
				return;
			}
			// Check to make sure we are going to a track and that track is the correct orientation
			PlaceableObject targetObj = manager.currLevel.rLayer.FindObject(this.GetPos() + MoveVector);
			// Error: rail we are going to is either none existant or the wrong direction
			if (!(targetObj is TrackObject tTargetObj) || tTargetObj.GetDir() != targetDir) {
				ui.setError(0, E.Name);
				Vector2I gridPos = this.GetCurrPos();
				Vector2I pixelPos = new Vector2I((gridPos.X) * tileSize, (gridPos.Y) * tileSize);
				Vector2I pixelPosWithOffset = new Vector2I((gridPos.X + 2) * tileSize, (gridPos.Y - 1) * tileSize);
				ui.setErrorCoords(pixelPosWithOffset);
				return;
			}
			
			Vector2I futureGridPosition = this.GetPos() + MoveVector;
			PlaceableObject existingClaw = manager.layerClaw.FindObject(futureGridPosition);
			if (existingClaw is ClawObject otherClaw && otherClaw != this)
			{
				ui.setError(2, E.Name);
				
				Vector2I pixelPosWithOffset = new Vector2I(
					(int)((futureGridPosition.X + 2) * tileSize),
					(int)((futureGridPosition.Y - 1) * tileSize));
				ui.setErrorCoords(pixelPosWithOffset);
				return;
			}


			/*ArrayList claws = manager.currLevel.cLayer.exportObjectList();
			foreach (var obj in claws)
			{
				if (obj is ClawObject clawObj)
				{
					if (clawObj == this) continue;

					Vector2 objPos = clawObj.GetCurrPos();
					Vector2 intendedPos = this.GetCurrPos() + MoveVector;

					GD.Print("comparing claws");
					GD.Print(objPos);
					GD.Print(intendedPos);

					if (objPos == intendedPos)
					{
						ui.setError(2, E.Name);

						Vector2I pixelPosWithOffset = new Vector2I((int)((intendedPos.X + 2) * tileSize),(int)((intendedPos.Y - 1) * tileSize));
						ui.setErrorCoords(pixelPosWithOffset);
						return;
					}
				}
			}*/

			MovingObject mObj = new MovingObject(this, MoveVector, manager.currLevel.cLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);

			return;
		}

		public void Grab(string[] args) {
			GD.Print("Grab func called");
			return; // TODO: implement fully
		}

		public void Drop(string[] args) {
			GD.Print("Drop func called");
			return; // TODO: implement fully
		}

		public void Rotate(string[] args) {
			return; // Throw error
		}

		// Command methods
		public ClawObject(int OGX, int OGY, int altTitle) : base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		~ClawObject() {
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
