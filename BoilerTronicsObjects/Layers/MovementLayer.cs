using Godot;
using System;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Layers
{
	public partial class MovementLayer : Layer
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
			// GD.Print("Selection: ", manager.currSlection, "| placing: ", manager.placingObject);
			if (manager.currSlection == 1)
			{
				if (manager.placingObject == 1) {
					if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsReleased())
					{
						Vector2 localMousePos = GetLocalMousePosition();
						Vector2I tileCoords = LocalToMap(localMousePos);

						// make sure nothing is there already
						PlaceableObject obj = FindObject(tileCoords);
						if (obj != null) {
							// reset so we don't place accidently
							manager.objectToPlace = new Vector2I(-1, -1);
							manager.placingObject = 0;

							if (manager.objectToMove != null) {
								AddObject(manager.objectToMove); // move the object back to it's original position
								manager.objectToMove = null;
							}

							// Check and see if we were moving
							
							base._Input(@event);
							return;
						}

						GD.Print("Factory layer is pressed");
						GD.Print("X: ", tileCoords.X, ", Y: ", tileCoords.Y);

						obj = manager.objectToMove;

						if (obj == null) {
							Vector2I atlasCords = manager.objectToPlace;
							AddObject(ObjectFactory.CreateObject(tileCoords, 2, atlasCords));
						} else {
							obj.MoveObject(tileCoords.X, tileCoords.Y); // move to the new position
							Vector2I newPos = obj.GetPos();
							AddObject(obj); // place object
						}

						// reset to prevent multiple placements
						manager.objectToMove = null;
						manager.objectToPlace = new Vector2I(-1, -1);
						manager.placingObject = 0;
					}
					else
					{
						base._Input(@event); // pass downward
					}
				} else {
					// Left mouse click on a spot where an object exitsts
					if (@event is InputEventMouseButton buttonEvent && buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsPressed()) {
						Vector2 localMousePos = GetLocalMousePosition();
						Vector2I tileCoords = LocalToMap(localMousePos);
						
						PlaceableObject obj = FindObject(tileCoords);

						// we don't went to do anything if we can;t find anything there
						if (obj == null) {
							base._Input(@event);
							return;
						}

						RemoveObject(obj);

						var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
						int sourceid = tileSet.GetSourceId(obj.GetSourceID());

						TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

						// get the tile
						var tile = tileSetSource.GetTileTextureRegion(obj.GetAtlasPos());
						var fullTexture = tileSetSource.Texture.GetImage();
						var imageTexture = fullTexture.GetRegion(tile);
						var texture = new ImageTexture();
						texture.SetImage(imageTexture);

						Sprite2D sprite = new Sprite2D();
						// get texture
						sprite.Texture = texture;
						sprite.Scale = new Vector2I(5, 5);
						sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(128, 128));

						var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, obj.GetAtlasPos());

						SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
						subView.AddChild(draggable);
						GD.Print("Created new dragable:", draggable);
						manager.objectToPlace = obj.GetAtlasPos();
						manager.objectToMove = obj; // this is so that we can move it back to it's origional position if the user places it in the incorrect spot

						manager.placingObject = 1;
					} else {
						base._Input(@event); // pass downward
					}
				}
			}
			else
			{
				// GD.Print("Recive event 2");
				base._Input(@event); // pass downward
			}
		}
	}
}
