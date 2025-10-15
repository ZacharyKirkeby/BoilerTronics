// TODO: implement in more detail
using Godot;
using System;
using System.Linq;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

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
			// TODO: Implement
			return null;
		}

		public void CreateTerminal() {
			// TODO: Implement
		}

		public void DestroyTerminal() {
			// TODO: Implement
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			MovingObject mObj = new MovingObject(this, new Vector2I(1,0), manager.currLevel.cLayer, 1);
			manager.currLevel.cLayer.GetParent().AddChild(mObj);

			return;
		}

		public void Grab(string[] args) {
		}

		public void Drop(string[] args) {
		}

		public void Rotate(string[] args) {
			// return error
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
