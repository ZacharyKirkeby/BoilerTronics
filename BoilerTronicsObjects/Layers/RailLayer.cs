using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	public partial class RailLayer : Layer
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
			// add a check to make sure that we are only trying to place rails (not claws)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

			// MouseInput(@event, 3, 1);
			if (!(manager.objectToMove is ClawObject)) {
				MouseInput(@event, 3);
				// GD.Print("Rail");
			}
			else if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right) && buttonEvent.IsPressed()) {
				// We always wnt to try to move
				GD.Print("Rail");
				MouseInput(@event, 3);
				return;
			}
			base._Input(@event);
		}
	}
}
