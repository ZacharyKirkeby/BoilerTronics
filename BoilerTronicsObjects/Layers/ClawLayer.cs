using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	// public partial class ClawLayer(int x, int y) : Layer(x, y)
	public partial class ClawLayer : Layer
	{

		public override void AddObject(PlaceableObject newPlaceable)
		{
			// TODO: add code to verify that this is the correct type of object
			AddObject(newPlaceable);
		}

		public override void RemoveObject(PlaceableObject objectToRemove)
		{
			// TODO: add code to verify that this is the correct type of object
			RemoveObject(objectToRemove);
		}

		public override void _Input(InputEvent @event)
		{
			MouseInput(@event, 3, 1);
			base._Input(@event);
		}
	}
}
