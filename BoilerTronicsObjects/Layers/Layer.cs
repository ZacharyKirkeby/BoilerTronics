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
													// TODO: add a bit mad for plocable areas
													// TODO: add a bit mad to show where stuff is already placed

		public void addObject(PlaceableObject newPlaceable)
		{
			if (newPlaceable == null) return; // make sure that the object isn't null
			objectList.Add(newPlaceable); // adds the placeable to the list of objects on this layer
			SetCell(newPlaceable.GetCurrPos(), newPlaceable.GetSourceID(), newPlaceable.GetAtlasPos()); // places new object
			numItems++;
		}

		public void removeObject(PlaceableObject objectToRemove)
		{
			if (!objectList.Contains(objectToRemove)) return;
			objectList.Remove(objectToRemove); // remove to object form the list
			EraseCell(objectToRemove.GetCurrPos()); // erase object from the map
			numItems--;
		}

		public PlaceableObject findObject(Vector2 loc)
		{
			for (int objIndex = 0; objIndex < numItems; objIndex++)
			{
				PlaceableObject obj = (PlaceableObject)objectList[objIndex];
				if (obj == null) continue; // The item in the list was not a placable object    
				Vector2 pos = obj.GetCurrPos();
				if (pos.X == loc.X && pos.Y == loc.Y) return obj; // We found the object!!
			}

			return null; // object was not found
		}

		public override void _Input(InputEvent @event)
		{
			// There should be checks in the above layer to see if we need to handle the click or not
			// --- Testing Code ---

			// Check if we have a left click event on the layer
			if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.Pressed)
			{
				Vector2 globalMousePos = GetViewport().GetMousePosition();
				Vector2 localMousePos = ToLocal(globalMousePos);
				Vector2I tileCoords = LocalToMap(localMousePos);

				GD.Print("X: ", tileCoords.X,", Y: ", tileCoords.Y);
			}
			// base._Input(@event); // Calling this will pass down the input, we want to absorbe it
		}

	}
}
