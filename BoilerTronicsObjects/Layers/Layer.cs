using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	public partial class Layer : Godot.TileMapLayer
	{
		ArrayList objectList = new ArrayList();     // List of objects that exist on the layer
		int numItems = 0;                           // Number of items in this layer

		public void addObject(PlaceableObject newPlaceable)
		{
			if (newPlaceable == null) return;
			objectList.Add(newPlaceable); // adds the placeable to the list of objects on this layer
			numItems++;
		}

		public void removeObject(PlaceableObject objectToRemove)
		{
			if (!objectList.Contains(objectToRemove)) return;
			objectList.Remove(objectToRemove); // remove to object form the list
			numItems--;
		}

		public PlaceableObject findObject(Vector2 loc)
		{
			for (int objIndex = 0; objIndex < numItems; objIndex++)
			{
				PlaceableObject obj = (PlaceableObject)objectList[objIndex];
				if (obj == null) continue; // The item in the list was not a placable object    
				Vector2 pos = obj.getPos();
				if (pos.X == loc.X && pos.Y == loc.Y) return obj; // We found the object!!
			}
			
			return null; // object was not found
		}
	}
}
