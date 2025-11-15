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
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public MovementLayer() : base() {
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		// if the new placeable object is a ConveyorObject, insert them into the ConveyorGroup
		// ConveyorGroups are "lines" of rails; if grouped together, this makes handling scripting and etc much more reasonable
		// Note: ConveyorGroups must also specifically update the heads of each list to hold also hold a reference to this group's CodeEdit
		// Note: what happens when we merge together two objects that already have contents in their terminals? Does one just take precedent? Probably yes
		public override void AddObject(PlaceableObject newPlaceable)
		{
			base.AddObject(newPlaceable);
		}

		public override void RemoveObject(PlaceableObject objectToRemove)
		{
			base.RemoveObject(objectToRemove);
		}
		

		public override void _Input(InputEvent @event)
		{
			MouseInput(@event, 1);
			base._Input(@event);
		}
	}
}
