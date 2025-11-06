using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects.MovementLayerObjects;

namespace BoilerTronicsObjects.Layers
{
	// public partial class MovementLayer(int x, int y) : Layer(x, y)
	public partial class MovementLayer : Layer
	{		

		public ArrayList ConvGroupList;

		public MovementLayer(int x, int y) : base(x,y) {
			ConvGroupList = new ArrayList(); // Create a list of groups
			
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public MovementLayer() : base() {
			ConvGroupList = new ArrayList(); // Create a list of groups
			
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public ConveyorGroup GetGroup(ConveyorObject cObj) {
			foreach (ConveyorGroup cGroup in ConvGroupList) {
				if (cGroup.Contains(cObj)) return cGroup;
			}

			return null;
		}
		

		// if the new placeable object is a ConveyorObject, insert them into the ConveyorGroup
		// ConveyorGroups are "lines" of rails; if grouped together, this makes handling scripting and etc much more reasonable
		// Note: ConveyorGroups must also specifically update the heads of each list to hold also hold a reference to this group's CodeEdit
		// Note: what happens when we merge together two objects that already have contents in their terminals? Does one just take precedent? Probably yes
		public override void AddObject(PlaceableObject newPlaceable)
		{
			base.AddObject(newPlaceable);
			
			// handle cases where the object fails to place correctly -- specifically, for ConveyorObjects
			// *should* solve issues where a terminal is created even when it shouldn't have
			if (!objectList.Contains(newPlaceable)) {
				return;
			}

			if (newPlaceable is ConveyorObject cObj) {
				GD.Print("MovementLayer: Inserting into Conveyor");
				
				// get ArrayList of adjacent objectes
				ArrayList conns = cObj.GetConnections();

				GD.Print("cons: ", conns.Count);

				if (conns.Count == 0) {
					// New group
					ConveyorGroup conveyorGroup = new ConveyorGroup(0, 0, cObj.GetDir());
					conveyorGroup.AddConveyor(cObj);
					ConvGroupList.Add(conveyorGroup);
					
					// update the head of the ConveyorGroup to hold the terminal information
					((ConveyorObject) cObj).SetTerminal(conveyorGroup.GetTerminal());
					
				} else if (conns.Count == 1) {
					// Add to group
					ConveyorGroup group = GetGroup(conns[0] as ConveyorObject);
					group.AddConveyor(cObj);
					
					// update the head of the ConveyorGroup to hold the terminal information, clears everything else
					// this might be excessive as this iterates through the whole group, cleans everything up
					// before then setting the head. 'AddConveyor' should only ever add to tail? but just to be safe I guess.
					group.ResetContentsTerminal();
				} else {
					// Count = 2
					// Combine groups (smaller to larger)
					ConveyorGroup g1 = GetGroup(conns[0] as ConveyorObject);
					ConveyorGroup g2 = GetGroup(conns[1] as ConveyorObject);

					if (g1.convList.Count > g2.convList.Count) {
						// Combine
						g1.AddConveyor(cObj);
						g1.CombineGroup(g2);

						// delete
						g2.UnRegisterSteppable();
						g2.DestroyTerminal();
						ConvGroupList.Remove(g2);
						
						// update the head of the ConveyorGroup to hold the terminal information, clears everything else
						g1.ResetContentsTerminal();
					} else {
						// Combine
						g2.AddConveyor(cObj);
						g2.CombineGroup(g1);

						// delete
						g1.UnRegisterSteppable();
						g1.DestroyTerminal();
						ConvGroupList.Remove(g1);
						
						// update the head of the ConveyorGroup to hold the terminal information, clears everything else
						g2.ResetContentsTerminal();
					}
				}
			}

			GD.Print("Num Groups: ", ConvGroupList.Count);
		}

		public override void RemoveObject(PlaceableObject objectToRemove)
		{
			base.RemoveObject(objectToRemove);

			if (objectToRemove is ConveyorObject cObj) {
				ConveyorGroup g = GetGroup(cObj);
				g.RemoveConveyor(cObj);
				if (g.convList.Count == 0) {
					g.UnRegisterSteppable();
					g.DestroyTerminal();
					ConvGroupList.Remove(g);
				}
				GD.Print("Num Groups: ", ConvGroupList.Count);
			}
		}
		

		public override void _Input(InputEvent @event)
		{
			MouseInput(@event, 1);
			base._Input(@event);
		}
	}
}
