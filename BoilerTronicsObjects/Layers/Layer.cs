using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.GameCamera;

namespace BoilerTronicsObjects.Layers
{
	public abstract partial class Layer : Godot.TileMapLayer
	{
		ArrayList objectList = new ArrayList();     // List of objects that exist on the layer
		int numItems = 0;                           // Number of items in this layer
													// TODO: add a bit mad for plocable areas
													// TODO: add a bit mad to show where stuff is already placed

		public virtual void AddObject(PlaceableObject newPlaceable)
		{
			if (newPlaceable == null) return; // make sure that the object isn't null
			objectList.Add(newPlaceable); // adds the placeable to the list of objects on this layer
			SetCell(newPlaceable.GetCurrPos(), newPlaceable.GetSourceID(), newPlaceable.GetAtlasPos()); // places new object
			// UpdateInternals();
			/*
			GD.Print("Placed object at: ", newPlaceable.GetCurrPos().X, " ", newPlaceable.GetCurrPos().Y);
			GD.Print("source ID: ", newPlaceable.GetSourceID());
			GD.Print("Atlas Coords: ", newPlaceable.GetAtlasPos().X, " ", newPlaceable.GetAtlasPos().Y);
			GD.Print("placed object");
			*/
			numItems++;
		}

		public virtual void RemoveObject(PlaceableObject objectToRemove)
		{
			if (!objectList.Contains(objectToRemove)) return;
			objectList.Remove(objectToRemove); // remove to object form the list
			EraseCell(objectToRemove.GetCurrPos()); // erase object from the map
			numItems--;
		}

		public virtual PlaceableObject FindObject(Vector2 loc)
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
			base._Input(@event);
		}

	}
}
