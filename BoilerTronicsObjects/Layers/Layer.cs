using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.GameCamera;
using BoilerTronicsObjects.Interfaces;

namespace BoilerTronicsObjects.Layers
{
	public partial class Layer : Godot.TileMapLayer
	{
		static BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// if false, then should block all drag attempts
		// NOTE: Currently unused; separate code already restricts dragging to when steps aren't running.
		public static bool allowDrag = true;
		
		// default layer dimensions, if left unspecified
		static int startX = 10;
		static int startY = 10;
		
		PlaceableObject[,] tiles;
		bool[,] editableTiles;
		
		// NOTE: "ArrayList" is apparently some old, mostly deprecated stuff in C#, unlike in Java where it's still very useful
		// Avoid using in the future!
		ArrayList objectList = new ArrayList();     // List of objects that exist on the layer
		int numItems = 0;                           // Number of items in this layer
		int maxX;
		int maxY;

		// General scaling for a given grabbed object
		static Vector2I grabbedObjectScaling = new Vector2I(1, 1);
		
		// Private bool to conditionally highlight a given tile
		private bool highlighting;
		private Vector2I highlightTarget;

		// note: this can't really be called by the child objects!
		public Layer(int x, int y) {
			RedefineLayer(x, y);
		}

		public Layer() {
			RedefineLayer(startX, startY);
		}
		
		// basically reconstructs the layer
		// mainly used because Layer(x, y) doesn't work unless the child object explicitly calls only that constructor (?)
		// x, y are # of cells on the respective axis
		public void RedefineLayer(int newX, int newY) {
			
			tiles = new PlaceableObject[newX, newY];
			editableTiles = new bool[newX, newY];
			maxX = newX - 1;
			maxY = newY - 1;
			
			// clear objects (let garbage collector handle the objects)
			// TODO: potential memory leak here or?
			objectList = new ArrayList();
			numItems = 0;
			
			// reset visuals
			Clear();

			for (int x = 0; x <= maxX; x++) {
				for (int y = 0; y <= maxY; y++) {
					editableTiles[x, y] = true;
				}
			}
		}
		
		// TODO: return this data safely rather than just returning the address
		// TODO: refactor to properly follow C# syntax
		public PlaceableObject[,] exportTiles() {
			return tiles;
		}
		public bool[,] exportEditableTiles() {
			return editableTiles;
		}
		public ArrayList exportObjectList() {
			return objectList;
		}
		public Vector2I exportDimensions() {
			return new Vector2I(maxX, maxY);
		}
		
		public void SetEditable(bool[,] editableTable) {
			if (!(editableTable.Length == (maxX + 1) * (maxY + 1))) return; // makes sure that the label has the same numbe of elements

			for (int x = 0; x <= maxX; x++) {
				for (int y = 0; y <= maxY; y++) {
					editableTiles[x, y] = editableTable[x, y];
				}
			}
		}
		
		// given coordinates, set a specific coordinate to 'value'
		// returns if operation was successful
		public bool SetTileEditable(Vector2I coordinates, bool value) {
			// check if OOB
			if (coordinates.X > maxX || coordinates.Y > maxY) { return false; }
			if (coordinates.X < 0 || coordinates.Y < maxY) { return false; }
			
			// if not OOB, then set value
			editableTiles[coordinates.X, coordinates.Y] = value;
			return true;
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
			GD.Print("Added object");
			
			// update placeable's parent layer info
			newPlaceable.SetParentLayer(this);
			
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

		public void Reset() {
			foreach (PlaceableObject obj in objectList) {
				Vector2I OldPos =  obj.GetPos();
				tiles[OldPos.X, OldPos.Y] = null;
				EraseCell(OldPos); // erase object from the map
				obj.ResetPos();
				Vector2I NewPos = obj.GetPos();
				tiles[NewPos.X, NewPos.Y] = obj;
				SetCell(NewPos, obj.GetSourceID(), obj.GetAtlasPos()); // places new object
			}
		}
		
		// Given a bool to enable/disable a single tile highlight and a Vector2I for the map coordinates of the specified tile,
		// enable/disable tile highlighting. If "highlight" is set to 'false', disables highlights automatically, regardless of the value of 'loc'.
		public void HighlightTile(bool highlight, Vector2I loc) {
			
			highlighting = highlight;
			
			if (highlight) {
				highlightTarget = loc;
			}
			
			// check: is this instance queued for deletion -- needed to mitigate debug spam!
			if (IsInstanceValid(this)) QueueRedraw();
		}
		
		public override void _Draw() {
			// GD.Print("Layer: trying to draw");
			// GD.Print("Highlight Position: ", highlightTarget);
			if (highlighting) {
				Vector2 localPos = MapToLocal(highlightTarget);
				// TODO: draw efficiently
				// for now, just create an array of Vector2
				Godot.Collections.Array coordinates = new Godot.Collections.Array();
				
				// generate a polygonal shape
				coordinates.Add(new Vector2(-20, -10));
				coordinates.Add(new Vector2(0, -20));
				coordinates.Add(new Vector2(20, -10));
				
				coordinates.Add(new Vector2(20, 10));
				coordinates.Add(new Vector2(0, 20));
				coordinates.Add(new Vector2(-20, 10));
				
				Color drawColor = Colors.Green;
				float lineWeight = 3.0f;
				
				// draw connecting from 'i-1' to 'i'
				for (int i = 1; i < coordinates.Count; i++) {
					DrawLine(localPos + (Vector2) coordinates[i-1], localPos + (Vector2) coordinates[i], drawColor, lineWeight);
				}
				// draw from 'maxI' to 'minI'
				DrawLine(localPos + (Vector2) coordinates[coordinates.Count - 1], localPos + (Vector2) coordinates[0], drawColor, lineWeight);
			}
		}

		public void MouseInput(InputEvent @event, int targetSel, int atlasID)
		{
			// make sure that this is a mouse event
			if (!(@event is InputEventMouseButton buttonEvent)) {
				base._Input(@event);
				return;
			}

			// Get manager
			// BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager; // get the manager

			if (manager.currLevel.StepCount != 0) return; // Don't do anythin if we are stepping

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
					
					// queue redraw for highlighting after moving an object
					// QueueRedraw();
					// 'null' check to prevent errors
					if (manager.terminalContainer != null) manager.terminalContainer.UpdateSelectedTerminal();
				}
			} else {
				// Left mouse click on a spot where an object exitsts
				// Handles creating a new draggable object when clicking on a tile
				if (buttonEvent.ButtonIndex == MouseButton.Left && buttonEvent.IsPressed()
					&& allowDrag) {

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
					sprite.Scale = grabbedObjectScaling;
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
					if (objAtPos is Runnable) manager.currLevel.UnRegisterRunnable(objAtPos);
					if (objAtPos is Scriptable sObj) sObj.DestroyTerminal();
				}
			}
		}

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
		}

	}
}
