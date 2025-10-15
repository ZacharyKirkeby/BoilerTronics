using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {

	public class ConveyorRotatorObject : MovementLayerObjects, Scriptable, Runnable {

		static Vector2I objectAtlasPos = new Vector2I(0, 2);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public ConveyorRotatorObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}

		public void CreateTerminal() {
			// TODO: Implement
		}

		public CodeEdit GetTerminal() {
			// TODO: Implement
			return null;
		}

		public void DestroyTerminal() {
			// TODO: Implement
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
