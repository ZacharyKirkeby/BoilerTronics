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
			// add a check to make sure that we are only trying to place floors
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currSlection != 2) {return;}
			// GD.Print("FloorLayer: Working Input");
			/*
			// MouseInput(@event, 2, 0);
			if (manager.objectToMove is FloorLayerObject) {
				MouseInput(@event, 2);
				// GD.Print("Floor");
			}
			else if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right) && buttonEvent.IsPressed()) {
				// We always wnt to try to move
				// MouseInput(@event, 2);
				return;
			}
			base._Input(@event);
			*/
		}
		
		
		public override void PassedMouseInput(InputEvent @event) {
			// GD.Print("FloorLayer : Receive Passed Input Working");
			
			if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right)) {
				MouseInput(@event, 2);
			}
		}
	}
}
