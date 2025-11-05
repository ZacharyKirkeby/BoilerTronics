/*
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class SwitchObject : PlaceableObject, Scriptable, Runnable {

		static int layerSourceId = 2;
		// reminder that the sourceID corresponds to the sprite sheet for a given layer
		// and every layer will have their own sprite sheet. Consequently, layer-specific
		// objects will have identical sourceIds.

		static Vector2I objectAtlasPos = new Vector2I(0, 3);
		CodeEdit E;
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public SwitchObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
			RegisterSteppable(); // Registers this as a runnable with the level state
		} // create object

		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Switch";
			
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

		public void Step() {
			// Make a call to the parser
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.P.ParseGetLine(this, E, E.Text, manager.currLevel.StepCount, E.Name);
			E.HighlightLine(E.getLastHighlighted() + 1, new Color(1, 1, 1, 0.3f));
		}

		public void Reset() {
			base.ResetPos();
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			return; // Throw error
		}

		public void Grab(string[] args) {
			return; // Throw error
		}

		public void Drop(string[] args) {
			return; // Throw error
		}

		public void Rotate(string[] args) {
			// This will be the placeholder command for the switch command, this will be changed once the new interpreter system is pushed to main and pulled into this branch
			// Get element on first position (This may be both a claw and a rail)
			// Get element on second position (This may be both a claw and a rail)

			// Add a check to make sure that the claw is not moving (This will eventually be overhauled with a new movement system when I get time, but is not needed for this sprint)

			// Remove element 1 from their layer
			// Remove element 2 from their layer

			// Place element 1 where elemet 2 is placed
			// Place element 2 where elemet 1 is placed

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

*/
