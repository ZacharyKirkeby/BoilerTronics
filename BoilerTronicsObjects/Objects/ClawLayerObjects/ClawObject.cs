// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class ClawObject : ClawLayerObjects, Scriptable, Runnable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.

		// Runnable Interface
		public void Step() {
		}

		public void RegisterSteppable() {
		}

		public void UnRegisterSteppable() {
		}

		// Scriptable interface

		// Methods to get commands and execute commands
		public string GetNextCommand() {
			return null;
		}

		public ScriptableCommand GetCommand(string commandWord) {
			return null;
		}

		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return null;
		}

		public void CreateTerminal() {
		}

		public void DestroyTerminal() {
		}

		// Command methods
		public ClawObject(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, objectAtlasPos, altTitle) {
			CreateTerminal(); // We need to create a terminal so that the user can actually write a script
		} // create object
	}
}
