using Godot;
using System;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Interfaces {
	interface Movable {
		PlaceableObject PickUp(); // Allows for an object to pick up this object
		void Place(); // Places the object back on the right layer
	}
}
