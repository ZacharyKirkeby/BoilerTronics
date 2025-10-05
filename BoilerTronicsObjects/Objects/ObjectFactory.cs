using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects
{
	public class ObjectFactory
	{
		// note: as of the current implementation, this isn't really a good factory in the strictest sense
		public static PlaceableObject CreateObject(Vector2I originPos, int sourceId, Vector2I atlasPos) {
			// TODO: creator/main factory function
			// given which "sourceId" (i.e. which atlas map to pull from) -- this will determine the object's layer
			// and given the "atlasPos" (i.e. where on the atlas the object is) -- this will determine the identify of the object (i.e. how Terraria does it)
			
			int x = originPos[0];
			int y = originPos[1];
			// edit: originPos.X/originPos.Y should also work
			
			// (pretend there's some fancy switch/if statements that take in source iDs and atlasPos
			// to identify an actual object
			
			// For now, as a demo, just return a "ClawObject"
			return new ClawObject(x, y, 0);
		}
	}
}
