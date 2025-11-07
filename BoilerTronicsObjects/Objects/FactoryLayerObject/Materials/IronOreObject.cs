using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {

	public class IronOreObject : PlaceableObject, Movable {
		
		private static Vector2I objectAtlasPos = new Vector2I(1, 0);
		private static int layerSourceId = 9;

		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		// Moveable, this will allow us to pickup and drop off items
		public PlaceableObject PickUp() {
			// Remove ourselves from the layer we exist in (factory layer)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.fLayer.RemoveObject(this);
			return this; // What ever is grabing this should manage this objct and place it again at some point
		}
		
		public bool Place(PlaceableObject obj) {
			return false; // This is a material, you can't place anything in us
		}

		public override void ResetPos()
		{
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.fLayer.RemoveObject(this);
			this.MoveObject(-1, -1); // Move to an invalid position
			base.ResetPos();
		}

		public IronOreObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, layerSourceId, objectAtlasPos, altTitle) {}
	}
}
