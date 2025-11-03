using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
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
		
		// special case for PlaceableBig objects
		public bool CheckValidPos(int X, int Y, PlaceableBig obj)
		{
			// check base origin point
			if (X < 0 || X > maxX || Y < 0 || Y > maxY) return false;
			
			// iterate through 'obj' texture grid
			foreach (PlaceableBigData data in obj.GetTextureGrid()) {
				// check each individual data point
				Vector2I dataCoords = data.GetPosition(obj.GetPos());
				X = dataCoords.X;
				Y = dataCoords.Y;
				if (X < 0 || X > maxX || Y < 0 || Y > maxY) return false;
			}
			
			return true;
		}

		// system should handle PlaceableBig objects
		public virtual void AddObject(PlaceableObject newPlaceable)
		{
			if (newPlaceable == null) return; // make sure that the object isn't null
			Vector2I pos = newPlaceable.GetPos();
			
			// very minor optimization
			bool isPlaceableBig = (newPlaceable is PlaceableBig);
			
			// check ValidPos stuff differently for PlaceableBig
			// check: are coordinates in bounds?
			if (isPlaceableBig) {
				if (!CheckValidPos(pos.X, pos.Y, (PlaceableBig) newPlaceable)) return;
			} else {
				if (!CheckValidPos(pos.X, pos.Y)) return;
			}
			
			// check: are coordinates occupied?
			if (isPlaceableBig) {
				if (FindObject(pos, (PlaceableBig) newPlaceable) != null) return;
			} else {
				if (FindObject(pos) != null) return;
			}
			
			// check: are the coordinates editable?
			if (isPlaceableBig) {
				PlaceableBig obj = (PlaceableBig) newPlaceable;
				// iterate through 'obj' texture grid
				foreach (PlaceableBigData data in obj.GetTextureGrid()) {
					// check each individual data point
					Vector2I dataCoords = data.GetPosition(obj.GetPos());
					
					// if any of the tile positions are marked as "not editable", return
					if (!editableTiles[dataCoords.X, dataCoords.Y]) return;
				}
			} else {
				if (!editableTiles[pos.X, pos.Y]) return;
			}
			

			objectList.Add(newPlaceable); // adds the placeable to the list of objects on this layer
			tiles[pos.X, pos.Y] = newPlaceable;
			SetCell(newPlaceable.GetCurrPos(), newPlaceable.GetSourceID(), newPlaceable.GetAtlasPos()); // places new object
			
			// update tiles, cells to fill accordingly to the PlaceableBig data
			if (isPlaceableBig) {
				PlaceableBig obj = (PlaceableBig) newPlaceable;
				// iterate through expected tiles and fill data (tilemap, internal data structs) accordingly
				// the "origin" object will already be placed by the code above!
				foreach (PlaceableBigData data in obj.GetTextureGrid()) {
					// check each individual data point
					Vector2I dataCoords = data.GetPosition(obj.GetPos());
					TileTex tex = data.GetTileTex();
					
					// update tiles to point to the origin (reference)
					tiles[dataCoords.X, dataCoords.Y] = newPlaceable;
					
					// update tile grid 
					SetCell(dataCoords, tex.GetSourceID(), tex.GetAtlasPos());
				}
			}
			
			// UpdateInternals();
			GD.Print("Added object");
			
			// update placeable's parent layer info
			newPlaceable.SetParentLayer(this);
			
			numItems++;
		}

		// system also should properly handle PlaceableBig objects
		public virtual void RemoveObject(PlaceableObject objectToRemove)
		{
			if (!objectList.Contains(objectToRemove)) return;
			Vector2I pos = objectToRemove.GetPos();
			if (!editableTiles[pos.X, pos.Y]) return;
			
			objectList.Remove(objectToRemove); // remove to object form the list
			EraseCell(objectToRemove.GetCurrPos()); // erase object from the map
			tiles[pos.X, pos.Y] = null; // remove from the tiles
			
			// PlaceableBig case
			if (objectToRemove is PlaceableBig) {
				PlaceableBig obj = (PlaceableBig) objectToRemove;
				
				// iterate through appropriate tiles and delete accordingly
				foreach (PlaceableBigData data in obj.GetTextureGrid()) {
					// check each individual data point
					Vector2I dataCoords = data.GetPosition(obj.GetPos());
					
					// update tile grid
					EraseCell(dataCoords);
					
					// update tile references
					tiles[dataCoords.X, dataCoords.Y] = null;
				}
			}
			
			numItems--;
		}

		// returns the reference to the object at 'loc' position
		// returns 'null' if object either does not exist, or 'loc' is OOB.
		public virtual PlaceableObject FindObject(Vector2I loc)
		{
			if (!CheckValidPos(loc.X, loc.Y) ) return null;
			return tiles[loc.X, loc.Y];
		}
		
		// special case for PlaceableBig objects
		// i.e. perform checks iteratively for all of a PlaceableBig object's grid offsets and etc
		// returns 'null' if all of the tiles occupied by 'PlaceableBig' are unoccupied by anthing but said 'PlaceableBig' object
		// otherwise, returns the first tile occupied by an object other than the 'PlaceableBig' within its tiles (i.e. PlaceableBigData's stuff)
		public virtual PlaceableObject FindObject(Vector2I loc, PlaceableBig obj)
		{	
			// TODO: AddObject updated to already perform 'CheckValidPos' before 'FindObject' is called
			// should this still be executed or should this below call be removed for performance optimization?
			if (!CheckValidPos(loc.X, loc.Y, obj) ) return null;
			
			// iterate through 'obj' texture grid
			foreach (PlaceableBigData data in obj.GetTextureGrid()) {
				// check each individual data point
				Vector2I dataCoords = data.GetPosition(obj.GetPos());
				
				// if tile is occupied, return tile reference
				PlaceableObject objAt = tiles[dataCoords.X, dataCoords.Y];
				if (objAt != null && objAt != obj) return objAt;
			}
			
			return tiles[loc.X, loc.Y];
		}

		public virtual void UpdateObject(PlaceableObject obj) {
			if (!objectList.Contains(obj)) return; // We don't care about this objcet if
			if (FindObject(obj.GetCurrPos()) != obj) return; // Verify that the object is in the position we think it is in

			SetCell(obj.GetCurrPos(), obj.GetSourceID(), obj.GetAtlasPos()); // Update cell for that object
		}

		public void Reset() {
			ArrayList objsToRemove = new ArrayList();
			foreach (PlaceableObject obj in objectList) {
				Vector2I CurrPos =  obj.GetCurrPos();
				Vector2I OGPos =  obj.GetOGPos();
				tiles[CurrPos.X, CurrPos.Y] = null;
				EraseCell(CurrPos); // erase object from the map

				// PlaceableBig case, erase from map accordingly
				if (obj is PlaceableBig) {
					PlaceableBig objB = (PlaceableBig) obj;
					
					// iterate through appropriate tiles and delete accordingly
					foreach (PlaceableBigData data in objB.GetTextureGrid()) {
						// check each individual data point
						Vector2I dataCoords = data.GetPosition(objB.GetPos());
						
						// update tile grid
						EraseCell(dataCoords);
						
						// update tile references
						tiles[dataCoords.X, dataCoords.Y] = null;
					}
				}
				
				if (obj.GetGarbage() == true) {
					objsToRemove.Add(obj);
					continue;
				}
				obj.ResetPos();
				
				tiles[OGPos.X, OGPos.Y] = obj;
				SetCell(OGPos, obj.GetSourceID(), obj.GetAtlasPos()); // places new object
				
				
				// PlaceableBig case, update map accordingly
				if (obj is PlaceableBig) {
					PlaceableBig objB = (PlaceableBig) obj;
					// iterate through expected tiles and fill data (tilemap, internal data structs) accordingly
					// the "origin" object will already be placed by the code above!
					foreach (PlaceableBigData data in objB.GetTextureGrid()) {
						// check each individual data point
						Vector2I dataCoords = data.GetPosition(objB.GetPos());
						TileTex tex = data.GetTileTex();
						
						// update tiles to point to the origin (reference)
						tiles[dataCoords.X, dataCoords.Y] = obj;
						
						// update tile grid 
						SetCell(dataCoords, tex.GetSourceID(), tex.GetAtlasPos());
					}
				}
				
			}

			foreach (PlaceableObject obj in objsToRemove) {
				objectList.Remove(obj);
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

		public void MouseInput(InputEvent @event, int targetSel)
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
						manager.placingObject = 0;

						if (manager.objectToMove != null) {
							AddObject(manager.objectToMove); // move the object back to it's original position
							manager.objectToMove = null;
						}

						return;
					}


					// This will happen if we are mopving an object
					PlaceableObject obj = manager.objectToMove;

					obj.MoveObject(tileCoords.X, tileCoords.Y); // move to the new position
					Vector2I newPos = obj.GetPos();
					AddObject(obj); // place object

					// reset to prevent multiple placements
					manager.objectToMove = null;
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

					// get the tile texture
					Texture2D texture = objAtPos.GetTexture() as Texture2D;

					Sprite2D sprite = new Sprite2D();
					// get texture
					sprite.Texture = texture;
					sprite.Scale = grabbedObjectScaling;
					sprite.Set(Sprite2D.PropertyName.Position, new Vector2I(128, 128));

					var draggable = new DraggableObject(Position - GetGlobalMousePosition(), sprite, objAtPos);

					SubViewport subView = GetTree().Root.GetNode("/root/Node2D/MainVBox/TerminalLevelSplit/VBoxContainer/LevelContainer/SubViewport") as SubViewport;
					subView.AddChild(draggable);
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
