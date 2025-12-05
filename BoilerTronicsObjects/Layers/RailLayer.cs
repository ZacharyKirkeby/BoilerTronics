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
			// add a check to make sure that we are only trying to place rails (not claws)
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			if (manager.currSlection != 3) {return;}
			GD.Print("RailLayer: Working Input");
			// MouseInput(@event, 3, 1);
			
			if (manager.objectToMove is RailLayerObject || manager.selectedObject is RailLayerObject) {
				GD.Print("RailLayer: Found rail, immediately working");
				MouseInput(@event, 3);
				// GD.Print("Rail");
			}
			else if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right) && buttonEvent.IsPressed()) {
				// We always wnt to try to move
				// GD.Print("Rail");
				// MouseInput(@event, 3);
				// NOTE: the "claw" layer will perform its input first before "passing on" to RailLayer.cs
				//return;
			}
			base._Input(@event);
		}
		
		public override void PassedMouseInput(InputEvent @event) {
			GD.Print("RailLayer : Receive Passed Input Working");
			
			if (@event is InputEventMouseButton buttonEvent && (buttonEvent.ButtonIndex == MouseButton.Left || buttonEvent.ButtonIndex == MouseButton.Right) && buttonEvent.IsPressed()) {
				MouseInput(@event, 3);
			}
		}
	}
}
