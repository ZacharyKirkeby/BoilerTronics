using Godot;
using System;
using Parsing;
using System.Collections;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Objects.MovementLayerObjects {
	public class ConveyorGroup : PlaceableObject, Runnable, Scriptable{
		
		public ArrayList convList = new ArrayList(); // List of conveyor objects
		private CodeEdit E;
		private static Vector2I dummyAtlasPos = new Vector2I(0,0);
		public int dir;
		private Parser _parser;

		public ConveyorGroup(int OGX, int OGY, int dir, int altTitle = 0) : base(OGX, OGY, 0, dummyAtlasPos, altTitle) { // The actual texture should not matter, this just needs to be a placable so that we can register it with the game state
			this.dir = dir; // this is the direction that we want to group (ConveyorObject.Right || ConveyorObject.Left)
			CreateTerminal();
			RegisterSteppable();
		}

		// Add to conveyor group
		public void AddConveyor(ConveyorObject conv) {
			if (conv == null) return;
			else if (conv.GetDir() != dir) return; // Make sure it's the correct
			convList.Add(conv);
			// VerifyGroup(); // Not needed, conveyors will only be added if they are adjacent
		}

		// Remove from conveyor group
		public void RemoveConveyor(ConveyorObject conv)
		{
			if (conv == null) return;
			convList.Remove(conv);
			VerifyGroup();
		}
		
		public void SetParser(Parser parser)
		{
			this._parser = parser;
		}
		
		public Parser GetParser()
        {
			return this._parser;
        }

		// Verify Group
		public void VerifyGroup() {
			ArrayList converyorGroupList = new ArrayList();

			ArrayList seenConveyors = new ArrayList();

			foreach (PlaceableObject obj in convList) {
				if (!(obj is ConveyorObject cObj)) continue; // make sure we don't look at things that are not conveyors
				else if (seenConveyors.Contains(cObj)) continue; // make sure we don't look at things we've seen before

				ArrayList convGroup = new ArrayList();

				Stack convStack = new Stack();

				convStack.Push(cObj);

				ArrayList Connected;

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
				ArrayList keepList = new ArrayList();

				foreach (ArrayList group in converyorGroupList) {
					GD.Print("Group Count: ", group.Count);
					if (group.Count > currMax) {
						currMax = group.Count;
						keepList = group;
					}
				}

				if (keepList.Count == 0) return;

				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

				// For all the other groups
				foreach (ArrayList group in converyorGroupList) {
					if (group != keepList) {
						// Create new Group
						ConveyorGroup newGroup = new ConveyorGroup(0, 0, dir);

						// Add all nodes to that group
						// Remove those nodes from us
						foreach (ConveyorObject cObj in group) {
							GD.Print("Adding item: ", cObj);
							newGroup.AddConveyor(cObj);
							manager.currLevel.mLayer.ConvGroupList.Add(newGroup);
							this.convList.Remove(cObj);
						}
					}
				}
			}
		}

		// Split Group
		public void SplitGroup() {
			// TODO
		}

		// Combine Group
		public void CombineGroup(ConveyorGroup g) {
			// Add all convs from g to our list
			if (g.dir != this.dir) return;

			foreach (ConveyorObject obj in g.convList) {
				this.convList.Add(obj);
			}
		}

		public bool Contains(ConveyorObject c) {
			return convList.Contains(c);
		}
		
		// Iterates through all child ConveyorObjects, clears their CodeEdit E fields
		// then sets the very first terminal item to have this group's CodeEdit E stored.
		// Should only be used by MovementLayer.cs
		public void ResetContentsTerminal() {
			GD.Print("ConveyorObject: ResetContentsTerminal()");
			foreach (ConveyorObject obj in convList) {
				obj.SetTerminal((CodeEdit) null);
			}
			((ConveyorObject) convList[0]).SetTerminal(E);
		}
		
		// Iterates through all child ConveyorObjects, clears their toLoad strings
		// if any toLoad string is not 'null', save it + source object
		// Should only be used by BoilerTronicsLevel
		public void LoadTerminal() {
			string terminalText = null;
			ConveyorObject target = null;
			
			// iterate through whole list
			foreach (ConveyorObject obj in convList) {
				string temp = obj.GetToLoadText();
				if (temp != null) {
					GD.Print("ConveyorGroup: found valid text to load: ", temp);
					terminalText = temp;
					target = obj;
					continue;
				}
				obj.SetToLoadText(null);
			}
			
			// if necessary, load the script into the terminal
			if (terminalText != null) {
				GD.Print("ConveyorGroup: loading script from save into terminal");
				SetScript(terminalText);
				target.SetToLoadText(null);
			}
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
			foreach (PlaceableObject obj in convList) obj.ResetPos(); // Reset each of our objects
		}

		// Scriptable interface


		// Methods to deal with terminals
		public CodeEdit GetTerminal() {
			return E;
		}

		public void CreateTerminal() {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			E = manager.terminalContainer.AddEditor();
			E.Name = "Conveyor";
			
			E.SetCorrespondingObject(this);
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

	}
}
