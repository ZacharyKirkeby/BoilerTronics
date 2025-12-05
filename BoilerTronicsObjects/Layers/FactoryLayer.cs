using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	// public partial class FactoryLayer(int x, int y) : Layer(x, y)
	public partial class FactoryLayer : Layer
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
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currSlection != 2) {return;}
			

			if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right)) {
				
				// if held/selected object is a floor layer object, pass mouse input to floor layer object
				// bypasses de-selection problems
				if (manager.objectToMove is FloorLayerObject || manager.selectedObject is FloorLayerObject) {
					// GD.Print("FactoryLayer: Found FloorLayerObject, passing through mouse input.");
					manager.layerFloor.PassedMouseInput(@event);
					return;
				}
				
				// GD.Print("FactoryLayer: Input Working");
				bool output = MouseInput(@event, 2);
				
				// if we didn't select anything successfully, pass down
				if (output == false) {
					// GD.Print("FactoryLayer: Passing mouse input to FloorLayer.cs");
					manager.layerFloor.PassedMouseInput(@event);
				}
				return;
			}
			base._Input(@event);
		}
	}
}
