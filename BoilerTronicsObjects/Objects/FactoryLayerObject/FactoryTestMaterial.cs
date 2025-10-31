using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryTestMaterial : FactoryLayerObjects, Runnable, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 3); // This is a dummy sprinte | TODO: Change this (not for this tesing object but for the actual object)

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			return null;
		}
		
		public void Place() {
		}

		// Runnable, this will allow the factory to do work
		public void Step() {

		}

		public void Reset() {

		}

		public void RegisterSteppable() {

		}

		public void UnRegisterSteppable() {

		}
		
		public FactoryTestMaterial(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
	}
}
