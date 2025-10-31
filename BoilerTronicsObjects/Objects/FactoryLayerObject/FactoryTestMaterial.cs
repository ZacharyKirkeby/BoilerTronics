using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class FactoryTestMaterial : FactoryLayerObjects, Movable {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 3); // This is a dummy sprinte | TODO: Change this (not for this tesing object but for the actual object)

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			// Remove ourselves from the layer we exist in (factory layer)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.fLayer.RemoveObject(this);
			return this; // What ever is grabing this should manage this objct and place it again at some point
		}
		
		public void Place(PlaceableObject obj) {
			return; // This is a material, you can't place anything in us
		}

		public FactoryTestMaterial(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
	}
}
