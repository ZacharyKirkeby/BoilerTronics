using Godot;
using System;
using Parsing;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;
using System.Collections.Generic;
using System.Collections;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {
	public class ConveyorGroup : PlaceableObject, Runnable, Scriptable, GroupedObject{
		
		private List<ConveyorObject> convList = new List<ConveyorObject>(); // List of conveyorObjects
		private static Vector2I dummyAtlasPos = new Vector2I(0,0);
		public int dir;
		private Parser _parser;
		private CodeEdit E;

		public ConveyorGroup(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, 0, dummyAtlasPos, altTitle) { // The actual texture should not matter, this just needs to be a placable so that we can register it with the game state
			GD.Print("New group");
			this.dir = dir; // this is the direction that we want to group (ConveyorObject.Right || ConveyorObject.Left)
			_parser = new Parser();
			_parser._Ready();
			CreateTerminal();
			RegisterSteppable();
		}

		// Grouped object interface

		// Gets the list of objects in this group
		public List<GroupedSubObject> getObjectList() {
			List<GroupedSubObject> retList = new List<GroupedSubObject>();

			foreach (ConveyorObject cObj in convList) {
				GroupedSubObject sgObj = cObj as GroupedSubObject;
				retList.Add(sgObj);
			}

			return retList;
		}

		// Checks the validity of the objects in it's list
		// It will return a list of any new grouped objects that are made in this verification process
		// This should be called after removing an object
		public List<GroupedObject> verifyGroup() {
			List<List<GroupedSubObject>> converyorGroupList = new List<List<GroupedSubObject>>();
			List<GroupedSubObject> seenConveyors = new List<GroupedSubObject>();
			List<GroupedObject> retList = new List<GroupedObject>();


			foreach (GroupedSubObject sObj in convList) {
				if (!(sObj is ConveyorObject cObj)) continue; // make sure we don't look at things that are not conveyors
				else if (seenConveyors.Contains(cObj)) continue; // make sure we don't look at things we've seen before

				List<GroupedSubObject> convGroup = new List<GroupedSubObject>();

				Stack convStack = new Stack();

				convStack.Push(cObj);

				List<ConveyorObject> Connected;

				while (convStack.Count != 0) {
					cObj = convStack.Pop() as ConveyorObject;

					seenConveyors.Add(cObj);
					convGroup.Add(cObj);

					Connected = cObj.GetConnections(); // These will all be the same direction

					foreach (ConveyorObject connObj in Connected) {
						if (!(seenConveyors.Contains(connObj))) convStack.Push(connObj); // If we haven't seen it, add it to the stack
					}
				}
				
				converyorGroupList.Add(convGroup);
			}
			
			if (converyorGroupList.Count != 1) {
				GD.Print("Split");
				// We need to split or we need to remove the group
				// Find the group with the most (this will be the one that we keep_
				int currMax = -1;
				List<GroupedSubObject> keepList = null;

				foreach (List<GroupedSubObject> group in converyorGroupList) {
					GD.Print("Group Count: ", group.Count);
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
							// GD.Print("Adding item: ", cObj);
							convList.Remove(gsObj as ConveyorObject);
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
			if (!(obj is ConveyorObject cObj)) return false; // We only want conveyor objects
			if (convList.Count == 0 && this.dir == cObj.GetDir()) return true; // If we don't have anything then we want to add

			List<ConveyorObject> conns = cObj.GetConnections(); // Get connections of conveyor
			foreach (ConveyorObject connCObj in conns) { // Go through each connection
				if (containsObject(connCObj)) return true; // If it's in our list then we are connected to this conveyor
			}

			return false; // We were not connected to this conveyor
		}

		// Adds object to the group
		// True if boject was added | False if object was not added
		public bool addObject(GroupedSubObject obj) {
			if (!validObject(obj)) return false;

			if (obj is ConveyorObject cObj) {
				convList.Add(cObj);
				cObj.setGroup(this);
				return true;
			}
			return false;
		}

		// Removes object from the group
		public void deleteObject(GroupedSubObject obj) {
			if (obj is ConveyorObject cObj && containsObject(cObj)) {
				convList.Remove(cObj);
				cObj.removeFromGroup();
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
			if (obj is ConveyorObject cObj) {
				return convList.Contains(cObj);
			}
			return false;
		}

		// Combines the group passed in with itself
		public void combineGroup(GroupedObject gObj) {
			List<GroupedSubObject> gsoList = gObj.getObjectList();

			foreach (GroupedSubObject gsObj in gsoList) {
				if (gsObj is ConveyorObject cObj) {
					gObj.deleteObject(cObj);
					addObject(cObj);
				}
			}
		}

		public void SetParser(Parser parser)
		{
			this._parser = parser;
		}
		
		public Parser GetParser()
		{
			return this._parser;
		}
		
		// Runnable Interface
		public void Step() {
			// Make a call to the parser
			GD.Print("Conveyor");
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (_parser == null)
			{
				GD.PrintErr($"{GetType().Name}: Parser not initialized!");
				return;
			}
			int highlight = _parser.ParseGetLine(this, E, E.Text, manager.currLevel.StepCount, E.Name);
			if (highlight >= 0) E.HighlightLine(highlight, new Color(1, 1, 1, 0.3f));
			UpdateRegisterDisplay();
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
			_parser.ResetProgramCounter();
			_parser.ResetRegisters();
			_parser.Reset();
			// foreach (PlaceableObject obj in convList) obj.ResetPos(); // Reset each of our objects
			E.ClearAllHighlights();
			var existing = E.GetNodeOrNull<Label>("ErrorLabel");
			if (existing != null)
			{
				existing.QueueFree();
			}
			UpdateRegisterDisplay();
		}

		// Scriptable interface


		// Methods to deal with terminals
		public CodeEdit GetTerminal()
		{
			return E;
		}
		
		private void UpdateRegisterDisplay()
		{
			var manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager?.terminalContainer == null) return;

			// Get the register label from the scene
			var terminalVBox = manager.terminalContainer.GetParent() as VBoxContainer;
			if (terminalVBox == null) return;

			var registerPanel = terminalVBox.GetNodeOrNull<PanelContainer>("RegisterPanel");
			if (registerPanel == null) return;

			var registerLabel = registerPanel.GetNodeOrNull<RegisterLabel>("RegisterLabel");
			if (registerLabel == null) return;

			// Only update if this terminal is currently visible
			var currentTerminal = manager.terminalContainer.GetCurrentTabControl();
			if (currentTerminal == E)
			{
				registerLabel.SetParser(_parser);
			}
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Terminals currTerminal = manager.terminalContainer;
			E = currTerminal.AddEditor();
			E.Name = "Conveyor";
			E.SetCorrespondingObject(this);
			
			// set as active tab
			// currTerminal.SetCurrentTab(currTerminal.GetTabCount() - 1);
			// update terminal highlighting
			// currTerminal.GetCurrentEditor().TerminalSelected();
		}

		public void DestroyTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.terminalContainer.RemoveEditor(E);
			E = null;
		}

		public void SetScript(string script) {
			E.Text = script;
		}

		public string GetScript() {
			return E.Text;
		}

		// Methods that we can use via commands
		public void Move(string[] args) {
			if (args == null) return;
			// else if (args[0] != "mov") return; // Not the correct command
			else if (args.Length != 2) return;

			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			Vector2I MoveVector;

			switch (args[1]) {
				case "u":
					if (this.dir == ConveyorObject.Right) return; // add error message
					MoveVector = new Vector2I(1, -1);
					GD.Print("up");
					break;
				case "d":
					if (this.dir == ConveyorObject.Right) return; // add error message
					MoveVector = new Vector2I(-1, 1);
					GD.Print("down");
					break;
				case "r":
					if (this.dir == ConveyorObject.Left) return; // add error message
					MoveVector = new Vector2I(1, 0);
					GD.Print("right");
					break;
				case "l":
					if (this.dir == ConveyorObject.Left) return; // add error message
					MoveVector = new Vector2I(-1, 0);
					GD.Print("left");
					break;
				default:
					GD.Print("invaid");
					return; // not a valid arg
			}

			// Loop through the objects in our list and call move with the vector passed in
			foreach (PlaceableObject obj in convList) {
				if (!(obj is ConveyorObject cObj)) continue;
				cObj.Move(MoveVector);
			}

			return;
		}

		public void Grab(string[] args) {
			return; // Throw error
		}

		public void Drop(string[] args) {
			return; // Throw error
		}

		public void Rotate(string[] args) {
			return; // Throw error
		}

		public void Switch(string[] args) {
			return; // Throw error
		}
	}
}
