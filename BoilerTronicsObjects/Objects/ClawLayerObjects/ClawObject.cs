// TODO: implement in more detail
using Godot;
using System;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Scriptable;

namespace BoilerTronicsObjects.Objects.ClawLayerObjects {

	public class ClawObject : ClawLayerObjects, ScriptableObject {
		
		static Vector2I objectAtlasPos = new Vector2I(0, 0);
		// "atlasPos" corresponds to the location on a given sprite sheet that a specific object
		// (i.e. "claw", "factory", "floor tile", "leftrail") will correspond to.
		
		public ClawObject(int OGX, int OGY, int altTitle = 0) 
		: base(OGX, OGY, objectAtlasPos, altTitle) {}
		
		
		// TODO: scriptable functions
		public bool Move(Vector2 movementVector) {
			return false;
		}
		public bool Grab() {
			return false;
		}
	}
}
