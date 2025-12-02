using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.GameCamera;
using BoilerTronicsObjects.Interfaces;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;

namespace BoilerTronicsObjects.Layers
{
	public partial class Layer : Godot.TileMapLayer
	{
		static BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// if false, then should block all drag attempts
		// NOTE: Currently unused; separate code already restricts dragging to when steps aren't running.
		public static bool allowDrag = true;
		
		// determines if system should render protected tiles or not
		private bool renderProtectedTiles = false;
		private bool editProtectedTiles = false;
		
		// default layer dimensions, if left unspecified
		static int startX = 10;
		static int startY = 10;
		
		PlaceableObject[,] tiles;
		bool[,] editableTiles;
		
		// for optimization purposes, keep track of all uneditable tiles
		private List<Vector2I> protectedList = new List<Vector2I>();
		
		// NOTE: "ArrayList" is apparently some old, mostly deprecated stuff in C#, unlike in Java where it's still very useful
		// Avoid using in the future!
		protected ArrayList objectList = new ArrayList();     // List of objects that exist on the layer
		protected ArrayList groupedObjectList = new ArrayList();     // List of objects that exist on the layer
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

			// sets all tiles to editable
			for (int x = 0; x <= maxX; x++) {
				for (int y = 0; y <= maxY; y++) {
					editableTiles[x, y] = true;
				}
			}
			protectedList.Clear();
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

		public ArrayList getGroupedList() {
			return groupedObjectList;
		}

		public void addGroupedObject(GroupedObject gObj) {
			if (gObj == null) return;
			groupedObjectList.Add(gObj);
		}
		
		// NOTE: unused?
		public void SetEditable(bool[,] editableTable) {
			if (!(editableTable.Length == (maxX + 1) * (maxY + 1))) return; // makes sure that the label has the same numbe of elements

			protectedList.Clear();
			for (int x = 0; x <= maxX; x++) {
				for (int y = 0; y <= maxY; y++) {
					// if protected tile, insert into protected list
					if (!editableTable[x, y]) {
						GD.Print("Layer.cs: SetEditable: ", "Adding to protectedList");
						protectedList.Add(new Vector2I(x, y));
						QueueRedraw();
					}
					editableTiles[x, y] = editableTable[x, y];
				}
			}
		}
		
		// given coordinates, set a specific coordinate to 'value'
		// returns if operation was successful
		public bool SetTileEditable(Vector2I coordinates, bool value) {
			// check if OOB
			if (!CheckInBounds(coordinates.X, coordinates.Y)) return false;
			
			// if not OOB, then set value
			editableTiles[coordinates.X, coordinates.Y] = value;
			
			bool protectedListContains = protectedList.Contains(coordinates);
			GD.Print("Layer.cs: protectedListContains: ", protectedListContains);
			
			// if this is to be a protected tile and it's not in the protected list,
			// then insert coords into the protected list!
			if (!value && !protectedListContains) {
				GD.Print("Layer.cs: SetTileEditable: ", "Adding to protectedList");
				protectedList.Add(coordinates);
				QueueRedraw();
				
			// else, if this is to be an editable tile and the protectedList contains the input coordinates,
			// then remove coords from the list!
			} else if (value && protectedListContains) {
				GD.Print("Layer.cs: SetTileEditable: ", "Removing from protectedList");
				protectedList.Remove(coordinates);
				QueueRedraw();
			}
			
			return true;
		}
		
		// check coordinates are within layer bounds
		public bool CheckInBounds(int X, int Y) {
			return !(X < 0 || X > maxX || Y < 0 || Y > maxY);
		}

		public bool CheckValidPos(int X, int Y)
		{
			if (!CheckInBounds(X, Y)) return false;
			if (!editableTiles[X, Y]) return false;
			return true;
		}
		
		// special case for PlaceableBig objects
		public bool CheckValidPos(int X, int Y, PlaceableBig obj)
		{
			// check base origin point
			if (!CheckInBounds(X, Y)) return false;
			if (!editableTiles[X, Y]) return false;
			
			Vector2I objOrigin = new Vector2I(X, Y);
			
			// iterate through 'obj' texture grid
			foreach (PlaceableBigData data in obj.GetTextureGrid()) {
				// check each individual data point
				Vector2I dataCoords = data.GetPosition(objOrigin); // data.GetPosition(obj.GetPos());
				X = dataCoords.X;
				Y = dataCoords.Y;
				// GD.Print("CheckValidPos: PlaceableBig case: ", dataCoords);
				if (!CheckInBounds(X, Y)) return false;
				if (!editableTiles[X, Y]) return false;
			}
			
			return true;
		}

		// system should handle PlaceableBig objects
		public virtual void AddObject(PlaceableObject newPlaceable)
		{
			// reset layer transparency
			//BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.layerClaw.Modulate = new Color(1, 1, 1, 1);
			manager.layerFactory.Modulate = new Color(1, 1, 1, 1);
			manager.layerFloor.Modulate = new Color(1, 1, 1, 1);
			manager.layerRail.Modulate = new Color(1, 1, 1, 1);
			manager.layerMovement.Modulate = new Color(1, 1, 1, 1);
			
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

			if (newPlaceable is GroupedSubObject gsObj) {
				List<GroupedObject> validGroups = new List<GroupedObject>();

				foreach (GroupedObject gObj in groupedObjectList) {
					if (gObj.validObject(newPlaceable as GroupedSubObject)) validGroups.Add(gObj);
				}

				if (validGroups.Count == 0) {
					GroupedObject gObj = gsObj.createGroup();
					if (gObj != null && gObj.addObject(gsObj)) groupedObjectList.Add(gObj);
				} else if (validGroups.Count == 1) {
					validGroups[0].addObject(newPlaceable as GroupedSubObject);
				} else {
					// Multipe objects
					GroupedObject biggestGroup = validGroups[0];

					foreach (GroupedObject gObj in validGroups) {
						if (gObj.getObjectList().Count > biggestGroup.getObjectList().Count) biggestGroup = gObj;
					}

					biggestGroup.addObject(newPlaceable as GroupedSubObject);

					foreach (GroupedObject gObj in validGroups) {
						if (gObj != biggestGroup) biggestGroup.combineGroup(gObj);
						groupedObjectList.Remove(gObj);
					}
				}
			}
			
			// UpdateInternals();
			GD.Print("Added object");
			
			// update placeable's parent layer info
			newPlaceable.SetParentLayer(this);
			
			// update highlighting as object is placed
			if (newPlaceable is Scriptable) {
				GD.Print("Layer.cs: calling to terminal to highlight");
				BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
				Terminals currTerminal = manager.terminalContainer;
				currTerminal.GetCurrentEditor().TerminalSelected();
			}
			
			numItems++;

			//BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			LevelUi ui = GetTree().Root.GetNodeOrNull<LevelUi>("Node2D");

			int costToAdd = newPlaceable.GetCost();
			if (costToAdd > 0) {
				GD.Print("Layer.cs: cost to add > 0, obj: ", newPlaceable);
			}
			
			if (newPlaceable is FactoryOutputObject ) {
				manager.currLevel.targetProduction = ((FactoryOutputObject) newPlaceable).getTargetNum();
				GD.Print("Layer.cs: found FactoryOutputObject, overriding manager.currLevel.targetProduction " + manager.currLevel.targetProduction);
			}
			/*
			switch (newPlaceable)
			{
				case ClawObject:
					costToAdd = 100;
					break;
				case TrackObject:
					costToAdd = 100;
					break;
				case ConveyorObject:
					costToAdd = 100;
					break;
				case ConveyorRotatorObject:
					costToAdd = 100;
					break;
				case FactoryInputObject:
					costToAdd = 100;
					break;
				case FactoryOutputObject f:
					costToAdd = 100;
					// BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
					// if (manager.currLevel != null) {
						// f.setTargetNum(manager.currLevel.targetProduction);
						// GD.Print("FactoryOutputObject added with goal: " + manager.currLevel.targetProduction);
					// }
					break;
				default:
					costToAdd = 0;
					break;
			}
			*/

			GD.Print("Layer.cs: added object path: ", GetPath());
			manager.currLevel.UpdateCost(costToAdd);
			ui?.UpdateCost(manager.currLevel.cost);
		}

		// system also should properly handle PlaceableBig objects
		public virtual void RemoveObject(PlaceableObject objectToRemove)
		{
			GD.Print(objectToRemove);
			if (!objectList.Contains(objectToRemove)) return;
			Vector2I pos = objectToRemove.GetPos();
			GD.Print(pos);
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
			
			if (objectToRemove is GroupedSubObject gsObj) {
				GroupedObject gObj = gsObj.getGroup();

				GD.Print("Group of object being deleted: ", gObj);
				GD.Print("Number of items in group before deletion: ", gObj.getObjectList().Count);

				if (groupedObjectList.Contains(gObj)) {
					GD.Print("Calling delete");
					gObj.deleteObject(objectToRemove as GroupedSubObject);
				}

				if (gObj.getObjectList().Count == 0) { // We are now empty
					if (gObj is Scriptable sObj) {
						sObj.DestroyTerminal();
					}

					if (gObj is Runnable rObj) {
						rObj.UnRegisterSteppable();
					}

					groupedObjectList.Remove(gObj);
					GD.Print("This group is now empty");
				} else {
					GD.Print("Number of items left in group: ", gObj.getObjectList().Count);
				}
			}

			numItems--;
			
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			LevelUi ui = GetTree().Root.GetNodeOrNull<LevelUi>("Node2D");

			int costToAdd = 0;
			switch (objectToRemove)
			{
				case ClawObject:
					costToAdd = -100;
					break;
				case TrackObject:
					costToAdd = -100;
					break;
				case ConveyorObject:
					costToAdd = -100;
					break;
				case ConveyorRotatorObject:
					costToAdd = -100;
					break;
				case FactoryInputObject:
					costToAdd = -100;
					break;
				case FactoryOutputObject:
					costToAdd = -100;
					break;
				default:
					costToAdd = 0;
					break;
			}

			GD.Print("deleting object");
			manager.currLevel.UpdateCost(costToAdd);
			ui?.UpdateCost(manager.currLevel.cost);
		}

		// returns the reference to the object at 'loc' position
		// returns 'null' if object either does not exist, or if 'loc' is OOB.
		public virtual PlaceableObject FindObject(Vector2I loc)
		{
			if (!CheckInBounds(loc.X, loc.Y)) return null;
			return tiles[loc.X, loc.Y];
		}
		
		// special case for PlaceableBig objects
		// i.e. perform checks iteratively for all of a PlaceableBig object's grid offsets and etc
		// returns 'null' if all of the tiles occupied by 'PlaceableBig' are unoccupied by anthing but said 'PlaceableBig' object
		// otherwise, returns the first tile occupied by an object other than the 'PlaceableBig' within its tiles (i.e. PlaceableBigData's stuff)
		public virtual PlaceableObject FindObject(Vector2I loc, PlaceableBig obj)
		{	
			// verify coordinates are within bounds
			if (!CheckInBounds(loc.X, loc.Y)) {return null;}
			
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

		// update an object's sprite position
		// NOTE: old coordinates are unaffected! whatever uses this MUST CORRECTLY DELETE THE OBJECT'S OLD SPRITES/POSITIONS, otherwise there'll be some odd visuals.
		public virtual void UpdateObject(PlaceableObject obj) {
			if (!objectList.Contains(obj)) return; // We don't care about this objcet if this object doesn't exist!
			if (FindObject(obj.GetCurrPos()) != obj) return; // Verify that the object is in the position we think it is in

			SetCell(obj.GetCurrPos(), obj.GetSourceID(), obj.GetAtlasPos()); // Update cell for that object
			
			// special: PlaceableBig case
			// update tiles, cells to fill accordingly to the PlaceableBig data
			if (obj is PlaceableBig) {
				PlaceableBig objB = (PlaceableBig) obj;
				// iterate through expected tiles and fill data (tilemap, internal data structs) accordingly
				// the "origin" object will already be placed by the code above!
				foreach (PlaceableBigData data in objB.GetTextureGrid()) {
					// check each individual data point
					Vector2I dataCoords = data.GetPosition(objB.GetCurrPos());
					TileTex tex = data.GetTileTex();
					
					// update tile grid 
					SetCell(dataCoords, tex.GetSourceID(), tex.GetAtlasPos());
				}
			}
			
			// end
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
		
		// Given the value of 'toggle', toggles on/off this layer rendering protected tiles
		// Then queues redrawing and etc
		public void HighlightProtectedTiles(bool toggle) {
			renderProtectedTiles = toggle;
			
			// check: is this instance queued for deletion -- needed to mitigate debug spam!
			if (IsInstanceValid(this)) QueueRedraw();
		}
		
		// Given the value of 'toggle', toggles on/off this layer allowing the editing of protected
		// tiles via keybinds (i.e. toggle behavior under _Input)
		public void EditProtectedTiles(bool toggle) {
			editProtectedTiles = toggle;
		}
		
		public override void _Draw() {
			// GD.Print("Layer: trying to draw");
			// GD.Print("Highlight Position: ", highlightTarget);
			
			// Highlight a terminal's corresponding object
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
			
			// Render projected tiles
			if (renderProtectedTiles) {
				RenderProtectedTiles();
			}
		}
		
		
		// specific, configuration y-offset for the below function
		protected int yRenderProtectedTileOffset = 25;
		
		// the function that actually renders protected tiles
		// "virtual" so offsets can be handled better by unique cases
		public virtual void RenderProtectedTiles() {
			// GD.Print("protectedList count: ", protectedList.Count);
			foreach (Vector2I coords in protectedList) {
				Vector2 localPos = MapToLocal(coords);
				// TODO: draw efficiently
				// for now, just create an array of Vector2
				Godot.Collections.Array coordinates = new Godot.Collections.Array();
				
				// generate a polygonal shape
				int yOffset = yRenderProtectedTileOffset;
				coordinates.Add(new Vector2(-18, 	yOffset + -9));
				coordinates.Add(new Vector2(0, 		yOffset + -18));
				coordinates.Add(new Vector2(18, 	yOffset + -9));
				coordinates.Add(new Vector2(0, 		yOffset + 0));
				
				Color drawColor = Colors.Blue;
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
					bool validPos = CheckValidPos(tileCoords.X, tileCoords.Y);
					if (manager.objectToMove is PlaceableBig) { // PlaceableBig case
						GD.Print("Placement: Checking PlaceableBig object");
						validPos = CheckValidPos(tileCoords.X, tileCoords.Y, (PlaceableBig) manager.objectToMove);
					}
					// make sure nothing is there already
					if (objAtPos != null || !validPos) {
						// reset so we don't place accidently
						GD.Print("Layer.cs: Invalid placement | ","X: ", tileCoords.X, ", Y: ", tileCoords.Y);
						manager.placingObject = 0;

						if (manager.objectToMove != null) {
							AddObject(manager.objectToMove); // move the object back to it's original position
							
							// handle cases where freshly spawned scriptable objects still create
							// a terminal, even if they should have been destroyed.
							if (!objectList.Contains(manager.objectToMove)) {
								if (manager.objectToMove is Scriptable) {
									GD.Print("Layer.cs: Destroying Terminal");
									// destroy terminal, unregister runnable
									((Scriptable) manager.objectToMove).DestroyTerminal();
									manager.currLevel.UnRegisterRunnable(manager.objectToMove);
								}
							}
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
					&& allowDrag)
				{

					// we don't went to do anything if we can;t find anything there
					if (objAtPos == null)
					{
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

					// when picking up an object, be sure to modulate the 
					// LAZY: modulate all layers
					manager.layerClaw.Modulate = manager.layerDeselectedVisibility;
					manager.layerFactory.Modulate = manager.layerDeselectedVisibility;
					manager.layerFloor.Modulate = manager.layerDeselectedVisibility;
					manager.layerRail.Modulate = manager.layerDeselectedVisibility;

					// unmodulate this layer
					this.Modulate = manager.layerDefaultVisibility;

				}
				else if (buttonEvent.ButtonIndex == MouseButton.Right && buttonEvent.IsPressed())
				{
					// We want to delete
					if (objAtPos != null) RemoveObject(objAtPos);
					if (objAtPos is Runnable) manager.currLevel.UnRegisterRunnable(objAtPos);
					if (objAtPos is Scriptable sObj) sObj.DestroyTerminal();
				}
			}
		}

		// very specific variable for a very specific purpose:
		// by introducing an offset to the mouse cursor, we can pick a tile that better selects a tile at where the cursor is *actually* looking at
		protected Vector2 protectedToggleMouseOffset = new Vector2(0f, -10f);
		
		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			
			// Handle keyboard inputs
			// Mainly used to handle toggling on/off protected tiles
			if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
				
				switch (keyEvent.Keycode) {
					// key codes: https://docs.godotengine.org/en/latest/classes/class_%40globalscope.html#enum-globalscope-key
					case Key.Up:
						
						// slightly offset the mouse position to get a better selected tile
						Vector2 localMousePos = GetLocalMousePosition() + protectedToggleMouseOffset;
						Vector2I tileCoords = LocalToMap(localMousePos);
						
						if (!editProtectedTiles) return; // exit immediately if not in "edit procted tiles" mode
						
						// toggle protected tile status at mouse position
						SetTileEditable(tileCoords, !(editableTiles[tileCoords.X, tileCoords.Y]));
						
						break;
				}
			}
		}

	}
}
