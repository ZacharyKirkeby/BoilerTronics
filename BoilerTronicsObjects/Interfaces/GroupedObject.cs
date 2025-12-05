using Godot;
using System;
using System.Collections.Generic;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Interfaces {
	/*
	 * This interface will allow for an object to consist of a grouping of subobjects
	 * All objects that are contained within a grouped object should have an interface 'GroupedSubObject' which is described below
	 * 
	 * The purpose for this grouping is to allow for an object to be able to exist of one or many of these grouped sub objects
	 * This allows for an object to be expanded and contracted based on the rules that that object will define in it's interface
	 */
	public interface GroupedObject {
		// Gets the list of objects in this group
		List<GroupedSubObject> getObjectList();

		// Checks the validity of the objects in it's list
		// It will return a list of any new grouped objects that are made in this verification process
		// This should be called after removing an object
		List<GroupedObject> verifyGroup();

		// Checks if adding this object is valid
		// Returns true if the object is able to be added
		// Returns false if the object can not be added
		bool validObject(GroupedSubObject obj);

		// Adds object to the group
		// True if boject was added | False if object was not added
		bool addObject(GroupedSubObject obj);

		// Removes object from the group
		void deleteObject(GroupedSubObject obj);

		// Checks if a given object is in the group
		bool containsObject(GroupedSubObject obj);

		// Combines the group passed in with itself
		void combineGroup(GroupedObject gObj);
	}

	/*
	 * The purpose of this interface is to allow us to check the group that a given grouped subobjec belongs to
	 * and for the grouped object to interact with it's grouped sub objects.
	 */
	public interface GroupedSubObject {
		// Creates group of correct type and returns that, does not put this item in that group
		GroupedObject createGroup();

		// Gets the group that this object belongs to
		GroupedObject getGroup();

		// Sets the group of the object
		void setGroup(GroupedObject gObj);

		// Removes object from group (sets some internal var to NULL)
		void removeFromGroup();

		// True if in group | False if not in group
		bool inGroup();

		/* These are used when the group is scriptable, we should try and find a way around this though if possible */

		// This should set some string var internally
		void setText(string T);

		// This should get some string variable internally
		string getText();
	}

	/* 
	 * The purpose of this interface is to allow for big placable objects to have sub objects that are also sub objects of groups
	 * This interface will allow for sub groups to detect adjacet sub objects that are internal in the big object
	 */
	public interface BigGroupedSubObject {
		GroupedSubObject getGroupedObject(Vector2I pos); // Get the sub object that is also a grouped object, should return null if one does not exist at pos
		List<GroupedSubObject> getObjects(); // Get all groupe subobjects in the big object (so we can make groups)
	}
}
