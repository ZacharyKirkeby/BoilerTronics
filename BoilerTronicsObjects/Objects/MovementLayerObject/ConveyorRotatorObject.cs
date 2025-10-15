using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorRotatorObject : MovementLayerObjects, Scriptable, Runnable {

		static Vector2I objectAtlasPos = new Vector2I(0, 2);
		CodeEdit E;
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public ConveyorRotatorObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}

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

		public void Step() {
			// TODO: Implement
			// Make a call to the parser
		}

		public void Reset() {
			// TODO: Implement
		}
		
		public void RegisterSteppable() {
			// TODO: Implement
		}

		public void UnRegisterSteppable() {
			// TODO: Implement
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			// return error
		}

		public void Grab(string[] args) {
			// return error
		}

		public void Drop(string[] args) {
			// return error
		}

		public void Rotate(string[] args) {
			// Rotate rail object below us
		}
	}
}
