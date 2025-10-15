using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {
	public class ConveyorGroup : PlaceableObject, Runnable, Scriptable{
		
		ArrayList convList = new ArrayList(); // List of conveyor objects
		private CodeEdit E;
		private static Vector2I dummyAtlasPos = new Vector2I(0,0);
		private int dir;

		public ConveyorGroup(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, 0, dummyAtlasPos, altTitle) { // The actual texture should not matter, this just needs to be a placable so that we can register it with the game state
			this.dir = dir; // this is the direction that we want to group (ConveyorObject.Right || ConveyorObject.Left)
		}


		// Runnable Interface
		public void Step() {
			// Make a call to the parser
			GD.Print("Conveyor");
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
			// Loop through elements in group and reset (shouldn't do anything)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			foreach (PlaceableObject obj in convList) obj.ResetPos(); // Reset each of our objects
		}

		// Scriptable interface


		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Conveyor";
			
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
			if (args == null) return;
			// else if (args[0] != "mov") return; // Not the correct command
			else if (args.Length != 2) return;

			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Vector2I MoveVector;

			switch (args[1]) {
				case "u":
					MoveVector = new Vector2I(1, -1);
					GD.Print("up");
					break;
				case "d":
					MoveVector = new Vector2I(-1, 1);
					GD.Print("down");
					break;
				case "r":
					MoveVector = new Vector2I(1, 0);
					GD.Print("right");
					break;
				case "l":
					MoveVector = new Vector2I(-1, 0);
					GD.Print("left");
					break;
				default:
					GD.Print("invaid");
					return; // not a valid arg
			}

			// Loop through the objects in our list and call move with the vector passed in
			foreach (PlaceableObject obj in convList) {
				if (!(obj is ConveyorObject cObj)) continue;
				cObj.Move(MoveVector);
			}

			return;
		}

		public void Grab(string[] args) {
			return; // Throw error
		}

		public void Drop(string[] args) {
			return; // Throw error
		}

		public void Rotate(string[] args) {
			return; // Throw error
		}

	}
}
