using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.GameCamera;

namespace BoilerTronicsObjects.Layers
{
	public partial class Layer : Godot.TileMapLayer
	{
		PlaceableObject[,] tiles;
		bool[,] editableTiles;
		ArrayList objectList = new ArrayList();     // List of objects that exist on the layer
		int numItems = 0;                           // Number of items in this layer
		int maxX;
		int maxY;
													// TODO: add a bit mad for plocable areas
													// TODO: add a bit mad to show where stuff is already placed
		public Layer(int x, int y) {
			tiles = new PlaceableObject[x, y];
			editableTiles = new bool[x, y];
			maxX = x - 1;
			maxY = y - 1;
		}

		public Layer() {
			tiles = new PlaceableObject[100, 100];
			editableTiles = new bool[100, 100];
			maxX = 99;
			maxY = 99;

			for (int x = 0; x <= maxX; x++) {
				for (int y = 0; y <= maxY; y++) {
					editableTiles[x, y] = true;
				}
			}
		}

		public bool CheckValidPos(int X, int Y)
		{
			if (X < 0 || X > maxX || Y < 0 || Y > maxY) return false;
			return true;
		}

		public virtual void AddObject(PlaceableObject newPlaceable)
		{
			if (newPlaceable == null) return; // make sure that the object isn't null
			Vector2I pos = newPlaceable.GetPos();
			if (FindObject(pos) != null) return;
			if (!CheckValidPos(pos.X, pos.Y)) return;

			objectList.Add(newPlaceable); // adds the placeable to the list of objects on this layer
			if (!editableTiles[pos.X, pos.Y]) return;
			tiles[pos.X, pos.Y] = newPlaceable;
			SetCell(newPlaceable.GetCurrPos(), newPlaceable.GetSourceID(), newPlaceable.GetAtlasPos()); // places new object
			// UpdateInternals();
			numItems++;
		}

		public virtual void RemoveObject(PlaceableObject objectToRemove)
		{
			if (!objectList.Contains(objectToRemove)) return;
			objectList.Remove(objectToRemove); // remove to object form the list
			EraseCell(objectToRemove.GetCurrPos()); // erase object from the map
			Vector2I pos = objectToRemove.GetPos();
			if (!editableTiles[pos.X, pos.Y]) return;
			tiles[pos.X, pos.Y] = null; // remove from the tiles
			numItems--;
		}

		public virtual PlaceableObject FindObject(Vector2I loc)
		{
			if (!CheckValidPos(loc.X, loc.Y) ) return null;
			return tiles[loc.X, loc.Y];
		}

		public void MouseInput(InputEvent @event, int targetSel, int atlasID)
		{
			// make sure that this is a mouse event
			if (!(@event is InputEventMouseButton buttonEvent)) {
				base._Input(@event);
				return;
			}

			// Get manager
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager; // get the manager

			// Get coords of event
			Vector2 localMousePos = GetLocalMousePosition();
			Vector2I tileCoords = LocalToMap(localMousePos);
			PlaceableObject objAtPos = FindObject(tileCoords);

			if (manager.currSlection != targetSel) return;

			if (manager.placingObject == 1) {
				if (buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsReleased())
				{
					// make sure nothing is there already
					if (objAtPos != null || !CheckValidPos(tileCoords.X, tileCoords.Y)) {
						// reset so we don't place accidently
						GD.Print("Invalid placement | ","X: ", tileCoords.X, ", Y: ", tileCoords.Y);
						manager.objectToPlace = new Vector2I(-1, -1);
						manager.placingObject = 0;

						if (manager.objectToMove != null) {
							AddObject(manager.objectToMove); // move the object back to it's original position
							manager.objectToMove = null;
						}

						return;
					}


					// This will happen if we are mopving an object
					PlaceableObject obj = manager.objectToMove;

					if (obj == null) {
						// Not moving, placing a new object
						Vector2I atlasCords = manager.objectToPlace;
						AddObject(ObjectFactory.CreateObject(tileCoords, atlasID, atlasCords));
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
			} else {
				// Left mouse click on a spot where an object exitsts
				if (buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsPressed()) {

					// we don't went to do anything if we can;t find anything there
					if (objAtPos == null) {
						return;
					}

					if (!editableTiles[tileCoords.X, tileCoords.Y]) return;

					RemoveObject(objAtPos);

					var tileSet = GD.Load<TileSet>("res://Resources/objects.tres");
					int sourceid = tileSet.GetSourceId(objAtPos.GetSourceID());

					TileSetAtlasSource tileSetSource = tileSet.GetSource(sourceid) as TileSetAtlasSource;

					// get the tile
					var tile = tileSetSource.GetTileTextureRegion(objAtPos.GetAtlasPos());
					var fullTexture = tileSetSource.Texture.GetImage();
					var imageTexture = fullTexture.GetRegion(tile);
					var texture = new ImageTexture();
					texture.SetImage(imageTexture);

					Sprite2D sprite = new Sprite2D();
					// get texture
					sprite.Texture = texture;
					sprite.Scale = new Vector2I(5, 5);
					sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(128, 128));

					var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, objAtPos.GetAtlasPos());

					SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
					subView.AddChild(draggable);
					manager.objectToPlace = objAtPos.GetAtlasPos();
					manager.objectToMove = objAtPos; // this is so that we can move it back to it's origional position if the user places it in the incorrect spot

					manager.placingObject = 1;
				} else if (buttonEvent.ButtonIndex == MouseButton.Right && buttonEvent.IsPressed()) {
					// We want to delete
					if (objAtPos != null) RemoveObject(objAtPos);
				}
			}
		}

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
		}

	}
}
