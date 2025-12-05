using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
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
			// do nothing -- we should only ever receive pass-through events fom the FactoryLayer!
			base._Input(@event);
		}
		
		
		public override void PassedMouseInput(InputEvent @event) {
			// GD.Print("FloorLayer : Receive Passed Input Working");
			
			if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right)) {
				MouseInput(@event, 2);
			}
		}
	}
}
