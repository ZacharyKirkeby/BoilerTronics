using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	// public partial class ClawLayer(int x, int y) : Layer(x, y)
	public partial class FloorLayer : Layer
	{

		public override void AddObject(PlaceableObject newPlaceable)
		{
			// TODO: add code to verify that this is the correct type of object
			base.AddObject(newPlaceable);
		}

		public override void RemoveObject(PlaceableObject objectToRemove)
		{
			// TODO: add code to verify that this is the correct type of object
			base.RemoveObject(objectToRemove);
		}

		public override void _Input(InputEvent @event)
		{
			// add a check to make sure that we are only trying to place floors
			// MouseInput(@event, 2, 0);
			base._Input(@event);
		}
	}
}
