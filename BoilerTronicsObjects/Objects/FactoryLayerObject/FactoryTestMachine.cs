using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryTestMachine : FactoryLayerObjects, Runnable, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 3); // This is a dummy sprinte | TODO: Change this (not for this tesing object but for the actual object)
	
		private PlaceableObject _Inv;
		private bool _Working;
		private int _StepsTillCompletion;

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			if (_Working) return null; // We don't let out inv be picked up till we are done with it

			PlaceableObject tmp = _Inv;
			_Inv = null;
			return _Inv;
		}
		
		public void Place(PlaceableObject obj) {
			_Inv = obj;
			_Working = true;
			_StepsTillCompletion = 2; // Wait 2 steps till we complete
		}

		// Runnable, this will allow the factory to do work
		public void Step() {
			if (!_Working) return;

			if (--_StepsTillCompletion == 0) _Working = false;
		}

		public void Reset() {
			_StepsTillCompletion = 0;
			_Working = false;
			if (_Inv != null) _Inv.ResetPos();
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			manager.currLevel.UnRegisterRunnable(this);
		}
		
		public FactoryTestMachine(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
	}
}
