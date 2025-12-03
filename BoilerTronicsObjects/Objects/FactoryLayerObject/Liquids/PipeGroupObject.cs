using Godot;
using System;
using Parsing;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;
using System.Collections.Generic;
using System.Collections;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.FactoryLayerObjects {
	public class PipeGroup : PlaceableObject, Runnable, GroupedObject{

		/*
		 * This will be used to track the liquid type in the system
		 * We should throw an error if these liquids do not exist in the system
		 */
		public enum LiquidType {
			None = 0,
			Water = 1,
			Lube = 2,
		}

		LiquidType currentType; // Current type of liquid
		int liquidAmount; // Current amout of liquid in the system
		
		private List<PipeObject> pipeList = new List<PipeObject>(); // List of conveyorObjects
		private static Vector2I dummyAtlasPos = new Vector2I(0,0);

		public void addLiquid(LiquidType T, int amt) {
			if (T == currentType) liquidAmount += amt;
			else {
				// Error: mixing liquid types
			}
		}

		public bool consumeLiquid(LiquidType T, int amt) {
			if (T == currentType && liquidAmount <= amt) liquidAmount += amt;
			else {
				return false; // Not correct type or not enough in system
			}

			return true; // Consumed liquid
		}

		public PipeGroup(int OGX, int OGY, int altTitle = 0) : base(OGX, OGY, 0, dummyAtlasPos, altTitle) { // The actual texture should not matter, this just needs to be a placable so that we can register it with the game state
			RegisterSteppable();
		}

		// Grouped object interface

		// Gets the list of objects in this group
		public List<GroupedSubObject> getObjectList() {
			List<GroupedSubObject> retList = new List<GroupedSubObject>();

			foreach (PipeObject pObj in pipeList) {
				GroupedSubObject sgObj = pObj as GroupedSubObject;
				retList.Add(sgObj);
			}

			return retList;
		}

		// Checks the validity of the objects in it's list
		// It will return a list of any new grouped objects that are made in this verification process
		// This should be called after removing an object
		public List<GroupedObject> verifyGroup() {
			List<List<GroupedSubObject>> converyorGroupList = new List<List<GroupedSubObject>>();
			List<GroupedSubObject> seenPipes = new List<GroupedSubObject>();
			List<GroupedObject> retList = new List<GroupedObject>();


			foreach (GroupedSubObject sObj in pipeList) {
				if (!(sObj is PipeObject pObj)) continue; // make sure we don't look at things that are not conveyors
				else if (seenPipes.Contains(pObj)) continue; // make sure we don't look at things we've seen before

				List<GroupedSubObject> pipeGroup = new List<GroupedSubObject>();

				Stack convStack = new Stack();

				convStack.Push(pObj);

				List<PipeObject> Connected;

				while (convStack.Count != 0) {
					pObj = convStack.Pop() as PipeObject;

					seenPipes.Add(pObj);
					pipeGroup.Add(pObj);

					Connected = pObj.GetConnections(); // These will all be the same direction

					foreach (PipeObject connObj in Connected) {
						if (!(seenPipes.Contains(connObj))) convStack.Push(connObj); // If we haven't seen it, add it to the stack
					}
				}
				
				converyorGroupList.Add(pipeGroup);
			}
			
			if (converyorGroupList.Count != 1) {
				// We need to split or we need to remove the group
				// Find the group with the most (this will be the one that we keep_
				int currMax = -1;
				List<GroupedSubObject> keepList = null;

				foreach (List<GroupedSubObject> group in converyorGroupList) {
					if (group.Count > currMax) {
						currMax = group.Count;
						keepList = group;
					}
				}

				if (keepList == null || keepList.Count == 0) return null;

				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

				// For all the other groups
				foreach (List<GroupedSubObject> group in converyorGroupList) {
					if (group != keepList) {
						// Create new Group
						GroupedObject newGroup = group[0].createGroup();

						// Add all nodes to that group
						// Remove those nodes from us
						foreach (GroupedSubObject gsObj in group) {
							pipeList.Remove(gsObj as PipeObject);
							newGroup.addObject(gsObj);
							retList.Add(newGroup);
						}
					}
				}
			}

			return retList;
		}

		// Checks if adding this object is valid
		// Returns true if the object is able to be added
		// Returns false if the object can not be added
		public bool validObject(GroupedSubObject obj) {
			if (!(obj is PipeObject pObj)) return false; // We only want conveyor objects
			if (pipeList.Count == 0) return true; // Empty list

			List<PipeObject> conns = pObj.GetConnections(); // Get connections of conveyor
			foreach (PipeObject connCObj in conns) { // Go through each connection
				if (containsObject(connCObj)) return true; // If it's in our list then we are connected to this conveyor
			}

			return false; // We were not connected to this conveyor
		}

		// Adds object to the group
		// True if boject was added | False if object was not added
		public bool addObject(GroupedSubObject obj) {
			if (!validObject(obj)) return false;

			if (obj is PipeObject pObj) {
				pipeList.Add(pObj);
				pObj.setGroup(this);

				foreach (PipeObject pConn in pObj.GetConnections()) {
					pConn.UpdateSprite();
				}

				pObj.UpdateSprite();

				return true;
			}

			return false;
		}

		// Removes object from the group
		public void deleteObject(GroupedSubObject obj) {
			if (obj is PipeObject pObj && containsObject(pObj)) {
				pipeList.Remove(pObj);
				pObj.removeFromGroup();

				foreach (PipeObject pConn in pipeList) {
					pConn.UpdateSprite();
				}
			}

			// Verify
			List<GroupedObject> newGroups = verifyGroup();
			if (newGroups == null) return;

			// add any newly created groups to our layer
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			// TODO: Create this in Layer.cs to allow for all layers to have grouped objects
			foreach (GroupedObject gObj in newGroups) manager.currLevel.mLayer.addGroupedObject(gObj);
		}

		// Checks if a given object is in the group
		public bool containsObject(GroupedSubObject obj) {
			if (obj is PipeObject pObj) {
				return pipeList.Contains(pObj);
			}
			return false;
		}

		// Combines the group passed in with itself
		public void combineGroup(GroupedObject gObj) {
			List<GroupedSubObject> gsoList = gObj.getObjectList();

			foreach (GroupedSubObject gsObj in gsoList) {
				if (gsObj is PipeObject pObj) {
					gObj.deleteObject(pObj);
					addObject(pObj);
				}
			}
		}
		
		// Runnable Interface
		public void Step() {
			// Make a call to the parser
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		}

		public void RegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.RegisterRunnable(this);
		}

		public void UnRegisterSteppable() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterRunnable(this);
		}

		public void Reset() {
			// Loop through elements in group and reset (shouldn't do anything)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		}
	}
}
