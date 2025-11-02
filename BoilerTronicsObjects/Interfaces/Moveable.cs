using Godot;
using System;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Interfaces {
	interface Movable {
		// Allows for an object to pick up this object
		// Also should be used by machines/factories to "give" objects to a claw
		PlaceableObject PickUp();
		
		// Places the given object on the corresponding layer
		// Also should be used by machines to "receive" dropped objects from claws
		bool Place(PlaceableObject obj);
	}
}
