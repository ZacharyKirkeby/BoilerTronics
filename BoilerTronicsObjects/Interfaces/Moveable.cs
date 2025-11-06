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

		// This is a function the input elemts will use to communicate to the parent big movable object
		// The obj should the the ibj should be the object the that user is inputing into the input
		// The ref object is a refrence to the object itself, this is so the the big movable can see what object is trying to give it an object
		bool GiveObject(PlaceableObject obj, PlaceableObject childObj);

		// This is a command will allow for an output to communicat with it's parent
		// It will request an object of a certian ID, if the machine has an object of that ID ready it will return the objet
		// Otherwise it'll return null
		// the chilObj is a refrence to the output that is calling this function on it's parent to be used for checking
		PlaceableObject RequestObject(int requestId, PlaceableObject childObj);
	}
}
