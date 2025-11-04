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

	/*
	 * This is the same as the other Movable interface
	 * The only diffrence is that the PickUp and Place methods take in a position
	 * The big object will check this position and use it to get the internal object that is at that position
	 * It will then eiter internally handle the input or output, or will pass it to that object which should also be a movable
	 */
	interface BigMovable {
		// Allows for an object to pick up this object
		// Also should be used by machines/factories to "give" objects to a claw
		PlaceableObject PickUp(Vector2I pos);
		
		// Places the given object on the corresponding layer
		// Also should be used by machines to "receive" dropped objects from claws
		bool Place(PlaceableObject obj, Vector2I pos);
	}
}
