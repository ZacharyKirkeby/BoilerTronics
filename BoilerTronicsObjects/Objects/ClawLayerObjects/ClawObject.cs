// TODO: implement in more detail
using Godot;
using System;
using System.Linq;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;
using Parsing;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class ClawObject : ClawLayerObjects, Scriptable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		private PlaceableObject heldObject = null;
		private CodeEdit E;

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.

		// Runnable Interface
		public void Step() {
			// Make a call to the parser
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.P.ParseGetLine(E.Text, manager.currLevel.StepCount, E.Name);
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
			// TODO: check movement vectors
			if (args == null) return;
			else if (args[0] != "mov") return; // Not the correct command
			else if (args.Length != 2) return;

			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Vector2I MoveVector;

			switch (args[1]) {
				case "u":
					MoveVector = new Vector2I(1, 0);
					break;
				case "d":
					MoveVector = new Vector2I(-1, 0);
					break;
				case "r":
					MoveVector = new Vector2I(0, 1);
					break;
				case "l":
					MoveVector = new Vector2I(0, -1);
					break;
				default:
					return; // not a valid arg
			}

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
		public ClawObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, objectAtlasPos, altTitle) {
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		~ClawObject() {
			DestroyTerminal(); // Destries the terminal for this scriptable
		}
	}
}
