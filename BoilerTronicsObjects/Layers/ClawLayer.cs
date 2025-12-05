using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	// public partial class ClawLayer(int x, int y) : Layer(x, y)
	public partial class ClawLayer : Layer
	{

		public override void AddObject(PlaceableObject newPlaceable)
		{
			// TODO: add code to verify that this is the correct type of object
			base.AddObject(newPlaceable);
			
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public override void RemoveObject(PlaceableObject objectToRemove)
		{
			// TODO: add code to verify that this is the correct type of object
			base.RemoveObject(objectToRemove);
			
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public override void _Input(InputEvent @event)
		{
			// add a check to make sure that we are only trying to place claws
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currSlection != 3) {return;}
			
			if (manager.objectToMove is ClawLayerObject || manager.selectedObject is ClawLayerObject) {
				GD.Print("ClawLayer: Found claw, working in.");
				MouseInput(@event, 3);
				// GD.Print("Claw");
			}
			else if (manager.objectToMove is RailLayerObject || manager.selectedObject is RailLayerObject) {
				GD.Print("ClawLayer: Found RailLayerObject, passing through mouse input.");
				manager.layerRail.PassedMouseInput(@event);
			}
			else if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right) && buttonEvent.IsPressed()) {
				// We always wnt to try to move
				GD.Print("ClawLayer: Input Working");
				bool output = MouseInput(@event, 3);
				
				
				if (output == false) {
					GD.Print("ClawLayer: Passing mouse input to RailLayer.cs");
					manager.layerRail.PassedMouseInput(@event);
				}
				return;
			}
			base._Input(@event);
		}
	}
}
