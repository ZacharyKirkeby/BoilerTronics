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
		}

		public MovementLayer() : base() {
			ConvGroupList = new ArrayList(); // Create a list of groups
		}

		public ConveyorGroup GetGroup(ConveyorObject cObj) {
			foreach (ConveyorGroup cGroup in ConvGroupList) {
				if (cGroup.Contains(cObj)) return cGroup;
			}

			return null;
		}

		public override void AddObject(PlaceableObject newPlaceable)
		{
			base.AddObject(newPlaceable);

			// if the new placeable object is a ConveyorObject, insert them into the ConveyorGroup
			if (newPlaceable is ConveyorObject cObj) {
				GD.Print("MovementLayer: Inserting into Conveyor");
				
				ArrayList conns = cObj.GetConnections();

				GD.Print("cons: ", conns.Count);

				if (conns.Count == 0) {
					// New group
					ConveyorGroup conveyorGroup = new ConveyorGroup(0, 0, cObj.GetDir());
					conveyorGroup.AddConveyor(cObj);
					ConvGroupList.Add(conveyorGroup);
				} else if (conns.Count == 1) {
					// Add to group
					ConveyorGroup group = GetGroup(conns[0] as ConveyorObject);
					group.AddConveyor(cObj);
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
					} else {
						// Combine
						g2.AddConveyor(cObj);
						g2.CombineGroup(g1);

						// delete
						g1.UnRegisterSteppable();
						g1.DestroyTerminal();
						ConvGroupList.Remove(g1);
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
			MouseInput(@event, 1, 2);
			base._Input(@event);
		}
	}
}
