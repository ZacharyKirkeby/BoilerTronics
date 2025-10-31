using Godot;
using System;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Interfaces {
	interface Movable {
		PlaceableObject PickUp(); // Allows for an object to pick up this object
		bool Place(PlaceableObject obj); // Places the object back on the right layer
	}
}
