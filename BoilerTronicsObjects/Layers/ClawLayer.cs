using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	public partial class ClawLayer : Layer
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
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager; // get the manager
			// Change this once UI is further along
			if (manager.currSlection == 3)
			{
				if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsReleased())
				{
					Vector2 localMousePos = GetLocalMousePosition();
					Vector2I tileCoords = LocalToMap(localMousePos);

					GD.Print("Claw layer is pressed");
					GD.Print("X: ", tileCoords.X, ", Y: ", tileCoords.Y);

					//testing; very primative method of moving the screen
					// this.Position += new Vector2(1, 1);

					// TODO: Pass in correct values here once factory is made
					// TODO: for now, create basic claw objects
					Vector2I atlasCords = manager.objectToPlace;
					AddObject(ObjectFactory.CreateObject(tileCoords, 1, atlasCords));
				}
				else
				{
					base._Input(@event); // pass downward
				}
			}
			else
			{
				base._Input(@event); // pass downward
			}
		}

	}
}
