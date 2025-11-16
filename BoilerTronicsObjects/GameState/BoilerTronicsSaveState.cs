using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Interfaces;	// get Scriptable interface
using BoilerTronicsObjects.Data;
using BoilerTronicsObjects.Objects; // get object factory
using BoilerTronicsObjects.Objects.MovementLayerObjects;	// ConveyorObject; essential exception to the norm that must be handled! (ConveyorGroup)

// NOTE TO SELF: c# apparently doesn't catch/immediately crash null pointer errors. fun.

// TODO: design distinct file system for auto saves, per level saves, etc

// TODO: the amount of forward-facing data is very jank in this object.
// and yes -- just to be clear -- I (Ethen C.) am VERY well aware that this code is not very clean at all
// and that interfacing with this code will be a mess.
// So please: just don't. Use the GlobalManager functions and call it a day.
// If you want to interface more with this object, then good luck.

public class BoilerTronicsSaveState
{
	// This should define what the save state of the current level is
	// We should serialize this and de-serialize it to save that level
	int save_slot = -1; // count from 0-2 for any given level save; autosave will have a '-1' save slot
	// TODO: handle errors if save_slot is OOB!
	
	int level_id = 0; // id for which level this save is referring to
	
	// TODO: implement into saving/loading
	public string levelName = "placeholder";
	
	// LAZY: this is public now
	// default: a 50% darker version of the base floor tile, 2 tiles wide
	public TileTex boundaryTex = new TileTex(new Vector2I(0, 0), 6);
	public int boundarySize = 2;
	
	private Vector2I levelDimensions = new Vector2I(20, 20);
	private LayerInfo liClaw = new LayerInfo();
	private LayerInfo liFactory = new LayerInfo();
	private LayerInfo liFloor = new LayerInfo();
	private LayerInfo liMovement = new LayerInfo();
	private LayerInfo liRail = new LayerInfo();
	
	// LAZY SAVE IMPLEMENTATION: save all the data completely raw.
	
	public BoilerTronicsSaveState() {
		// do nothing (for now)
	}
	
	// set/get level dims
	public void SetLevelDimensions(Vector2I max) {
		levelDimensions = max;
	}
	public Vector2I GetLevelDimensions() {
		return levelDimensions;
	}
	
	// set the level ID for this save
	// TODO: used only by the save system or also by the load system? probably both
	public void SetLevelID(int input) {
		level_id = input;
	}
	public int GetLevelID() {
		return level_id;
	}
	
	// set the save slot for this save
	// TODO: used only by the save system or also by the load system? probably both
	public void SetSaveSlot(int input) {
		save_slot = input;
	}
	public int GetSaveSlot() {
		return save_slot;
	}
	
	// quick easy way to do autosaves; may not be needed
	public void SaveAutosave(BoilerTronicsGlobalManager manager) {
		 SaveDataTo(manager, "", "auto");
	}
	
	// note: "Save Location" is based off the base save directory
	// i.e. '%user%/AppData/Roaming/Godot/app_userdata/[game name]/SaveLocationName'
	// note: 'fileName' should also have the slash beforehand!
	// note2: 'directory' should contain all directories/subdirectories up until the actual save file itself
	//
	// example for autosave: dir = "", fileName = "auto".save
	// example for level specific save: dir: "level#", fileName: "/save0".save
	// TODO: no longer using the 'using' keyword for the save file, need to double check that no memory leaks are created!
	public void SaveDataTo(BoilerTronicsGlobalManager manager, string directory, string fileName) {
		// lazy; import public data straight from manager
		
		// note: some formatting adopted from "https://docs.godotengine.org/en/stable/tutorials/io/saving_games.html"
		string DirectoryPath = "user://" + directory;
		string SavePath = DirectoryPath + fileName + ".save";
		GD.Print("SaveState: Trying to save to ", SavePath);
		// if (!FileAccess.FileExists(SavePath)) {return;} // not valid save location
		
		// TODO: should implement in such a way that safely copies over the information, but that'll be done later.
		// Just don't forget to do this! (security reasons, etc -- although who would try to hack this game via .dll injection and etc? Who knows.)
		
		// save all crucial data per layer
		liClaw = GenerateLayerInfo(manager.layerClaw, levelDimensions);
		liFactory = GenerateLayerInfo(manager.layerFactory, levelDimensions);
		liFloor = GenerateLayerInfo(manager.layerFloor, levelDimensions);
		liMovement = GenerateLayerInfo(manager.layerMovement, levelDimensions);
		liRail = GenerateLayerInfo(manager.layerRail, levelDimensions);
		
		// TODO: properly serialize tile object information
		/* 
			As currently we want a *functional* save system over an optimized one, objects will be
			serialized inefficiently, i.e. will save just raw data for now.
		*/
		
		// serialize object list (per layer), uneditable tile coordinate list (per layer).
		/*
		GD.Print("save json test (claw layer): ", Json.Stringify(liClaw.serializeData()));
		GD.Print("save json test (factory layer): ", Json.Stringify(liFactory.serializeData()));
		GD.Print("save json test (floor layer): ", Json.Stringify(liFloor.serializeData()));
		GD.Print("save json test (movement layer): ", Json.Stringify(liMovement.serializeData()));
		GD.Print("save json test (rail layer): ", Json.Stringify(liRail.serializeData()));
		*/
		
		// optimizations are cool and all but that's only viable if you have time, which is something I don't have.
		// store the raw data of the map size, per layer contents + uneditable tiles.
		/*
		Godot.Collections.Dictionary<string, Variant> output = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "mapSize", new int[]{levelDimensions.X, levelDimensions.Y} },
				{ "clawLayer", liClaw.SerializeData() },
				{ "factoryLayer", liFactory.SerializeData() },
				{ "floorLayer", liFloor.SerializeData() },
				{ "movementlayer", liMovement.SerializeData() },
				{ "railLayer", liRail.SerializeData() },
			};
		*/
		//GD.Print("save data: ", Json.Stringify(output));
		
		// by default saved in '%user%/AppData/Roaming/Godot/app_userdata/[game name]'
		// TODO: have a distinct file save system for saving autosaves, level saves, etc
		var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		// 'using' keyword means that this is automatically disposed of when going out of scope
		
		// if we can't open the file, then try and make the directory
		// and then try to open the file again
		if (saveFile == null) {
			GD.Print("SaveState: Could not save, err: ", FileAccess.GetOpenError());
			GD.Print("SaveState: Trying to create (recursive) directory(s) instead:");
			
			var dirSuccess = DirAccess.MakeDirRecursiveAbsolute(DirectoryPath);
			
			// if 'ERROR' == 0, then good. else, not so good.
			if (dirSuccess != 0) {
				GD.Print("SaveState: Failed to make recursive directory(s): " + DirectoryPath);
				return;
			}
			
			// try again
			saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			
			if (saveFile == null) {
				GD.Print("SaveState: Could not save, err: ", FileAccess.GetOpenError());
				GD.Print("SaveState: Aborting save process.");
				return;
			} else {
				GD.Print("SaveState: Successfully created recursive directories and save file. Continue saving process now.");
			}
		}
		
		BoilerTronicsGlobalManager man = BoilerTronicsGlobalManager.GlobalManager;
		
		Godot.Collections.Dictionary<string, Variant> metadata = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "mapSize", new int[]{levelDimensions.X, levelDimensions.Y} },
				{ "levelId", level_id },
				{ "levelName", levelName },
				{ "saveSlot", save_slot },
				{ "boundarySize", boundarySize },
			};
			
		if (this.boundaryTex != null) {
			metadata["boundaryAtlasX"] = boundaryTex.GetAtlasPos().X;
			metadata["boundaryAtlasY"] = boundaryTex.GetAtlasPos().Y;
			metadata["boundarySourceId"] = boundaryTex.GetSourceId();
		}
		
		// GD.Print("mapSize: " + metadata["mapSize"]);
		// GD.Print("levelId: " + metadata["levelId"]);
		// GD.Print("saveSlot: " + metadata["saveSlot"]);
		
		// now: save important information line-by-line!
		SaveLine(saveFile, "metadata", metadata);
		SaveLine(saveFile, "clawLayer", liClaw.SerializeData());
		SaveLine(saveFile, "factoryLayer", liFactory.SerializeData());
		SaveLine(saveFile, "floorLayer", liFloor.SerializeData());
		SaveLine(saveFile, "movementlayer", liMovement.SerializeData());
		SaveLine(saveFile, "railLayer", liRail.SerializeData());
		
		((FileAccess) saveFile).Close();
	}
	
	// load level data; automatically generate the save data info, given the level ID
	// input should be handled automatically by the global manager
	// returns success of loading the save data
	public bool LoadLevelData(BoilerTronicsGlobalManager manager, int levelId) {
		string SavePath = "res://Resources/Levels/level" + levelId + ".save";
		return LoadData(manager, SavePath);
	}
	
	// load level name; automatically generate the save data info, given the level ID
	// input should be handled automatically by the global manager
	// returns success of loading the save data
	public bool LoadLevelName(BoilerTronicsGlobalManager manager, String levelName) {
		string SavePath = "res://Resources/Levels/" + levelName + ".save";
		return LoadData(manager, SavePath);
	}
	
	
	// load save data; automatically generate the save data info, given the directory and etc
	// input should be handled automatically by the global manager
	// returns success of loading the save data
	public bool LoadSaveData(BoilerTronicsGlobalManager manager, string SaveLocationName) {
		string SavePath = "user://" + SaveLocationName + ".save";
		return LoadData(manager, SavePath);
	}
	
	// given a save location, load the data from that save and save that into our private data objects
	// returns success of loading the file
	private bool LoadData(BoilerTronicsGlobalManager manager, string SavePath) {
		
		if (!FileAccess.FileExists(SavePath)) {return false;} // not valid save location
		
		// open up save data
		using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		// 'using' keyword means that this is automatically disposed of when going out of scope
		
		// much copied from Godot's documentation
		while (saveFile.GetPosition() < saveFile.GetLength()) {
			var jsonString = saveFile.GetLine();

			// Creates the helper class to interact with JSON.
			var json = new Json();
			var parseResult = json.Parse(jsonString);
			if (parseResult != Error.Ok)
			{
				GD.Print($"SaveState: JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
				continue;
			}
			

			// Get the data from the JSON object.
			// TODO: advanced error checking
			var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			foreach (var (key, value) in nodeData)
			{
				// GD.Print("loading: " + key + "\nvalue: " + value);
				// cast 'value' into 'node'
				Godot.Collections.Dictionary<string, Variant> node = (Godot.Collections.Dictionary<string, Variant>) value;
				
				// metadata case; load in important data to this game state object
				if (key == "metadata") {
					this.level_id = (int) node["levelId"];
					this.save_slot = (int) node["saveSlot"];
					
					// cast map size into array
					Godot.Collections.Array mapSize = (Godot.Collections.Array) node["mapSize"];
					
					this.levelDimensions = new Vector2I((int) mapSize[0], (int) mapSize[1]);
					
					// if save data has "boundarySize", assume that it has all corresponding
					// boundary data
					if (node.ContainsKey("boundarySize")) {
						this.boundarySize = (int) node["boundarySize"];
						int atlasX = (int) node["boundaryAtlasX"];
						int atlasY = (int) node["boundaryAtlasY"];
						this.boundaryTex.SetAtlasPos(atlasX, atlasY);
						this.boundaryTex.SetSourceId((int) node["boundarySourceId"]);
					}
					
					if (node.ContainsKey("levelName")) {
						this.levelName = (string) node["levelName"];
					}
					
					GD.Print("SaveState: metadata: level:", + level_id + ", save slot:" + save_slot + ", level dimensions:" + levelDimensions);
					GD.Print("SaveState: metadata: level name: ", levelName);
					continue;
				}
				
				// handle layers; use helper function to minimize redundant code.
				if (key == "clawLayer") {
					LoadIntoLayer((Godot.Collections.Dictionary<string, Variant>) node, liClaw);
					continue;
				}
				if (key == "factoryLayer") {
					LoadIntoLayer((Godot.Collections.Dictionary<string, Variant>) node, liFactory);
					continue;
				}
				if (key == "floorLayer") {
					LoadIntoLayer((Godot.Collections.Dictionary<string, Variant>) node, liFloor);
					continue;
				}
				if (key == "movementlayer") {
					LoadIntoLayer((Godot.Collections.Dictionary<string, Variant>) node, liMovement);
					continue;
				}
				if (key == "railLayer") {
					LoadIntoLayer((Godot.Collections.Dictionary<string, Variant>) node, liRail);
					continue;
				}
			}
		}
		
		// successfully loading
		return true;
	}
	
	// used by 'LoadData()', helper function
	// TODO: input validation
	// updates a LayerInfo's  "objectList" and "protectedTiles" values
	private void LoadIntoLayer(Godot.Collections.Dictionary<string, Variant> input, LayerInfo Layer) {
		// NOTE: this function uses an unholy amount of casting
		// errors are VERY LIKELY TO OCCUR if input is not sanitized
		// TODO: how can we sanitize input? do we just have to assume that the save file is valid?
		// NOTE: if something doesn't cast right or etc, system just hangs up without an obvious crash. This is bad, no?
		
		// extract objects
		// each will be a dictionary that contains the following:
		// "OGX", "OGY", "altTitle", "atlasPosX", "atlasPosY", "sourceId"
		Godot.Collections.Array objects = (Godot.Collections.Array) input["objects"];
		
		// extract uneditable/protected tiles
		// array of 2d arrays representing a protected coordinate
		Godot.Collections.Array uneditableTiles = (Godot.Collections.Array) input["uneditableTiles"];
		// GD.Print("established 'objects', 'uneditableTiles'");
		GD.Print("SaveState: objects count: " + objects.Count + ", protected tiles count: " + uneditableTiles.Count);
		
		// handle objects first:
		// iterate through array and create the Placeable objects
		// insert these placeable objects into an ArrayList (old data struct, i know, but it's compatibility reasons and I'm too lazy to refactor atm)
		// insert them into the LayerInfo object
		// and whatever load functionality will get this ArrayList and then insert said objects appropriately in the list.
		// TODO: 'CreateObject' does not handle altTitles!
		// GD.Print("now reading objects from file");
		ArrayList listObj = new ArrayList();

		// TODO:
		// We need to create a list of groups and the associated code, then when all of the groups are crated,
		// we need to set the text after all the groups are correctly made

		for (int i = 0; i < objects.Count; i++) {
			// cast each object of the array into desired dictionary type
			Godot.Collections.Dictionary<string, Variant> targetObj = (Godot.Collections.Dictionary<string, Variant>) objects[i];
			Vector2I originPos = new Vector2I((int) targetObj["OGX"], (int) targetObj["OGY"]);
			Vector2I atlasPos = new Vector2I((int) targetObj["atlasPosX"], (int) targetObj["atlasPosY"]);
			
			// create object, add to array list
			PlaceableObject target = ObjectFactory.CreateObject(originPos, (int) targetObj["sourceId"], atlasPos);
			
			// if object is scriptable, attempt to load terminal code
			// TODO: implement similar functionality for ConveyorObject!
			if (target is Scriptable) {
				string terminalCode;
				
				if (targetObj.ContainsKey("terminalCode")) {
					// if loaded string exists, then load as appropriate
					terminalCode = (string) targetObj["terminalCode"];
					
					
					// TODO: cast object as appropriate and create appropriate terminal, load in data, etc
					
					((Scriptable) target).SetScript(terminalCode);
					GD.Print("SaveState: successfully loaded terminal code");
				}
			} else if (target is GroupedSubObject) {
				string groupCode;
				
				if (targetObj.ContainsKey("groupCode")) {
					// if loaded string exists, then load as appropriate
					groupCode = (string) targetObj["groupCode"];
					
					
					// TODO: cast object as appropriate and create appropriate terminal, load in data, etc
					((ConveyorObject) target).SetToLoadText(groupCode);
					GD.Print("SaveState: successfully loaded terminal code -- conveyor variant");
				}
			}
			
			// get direction, if relevant
			if (target is PlaceableBig) {
				if (targetObj.ContainsKey("dir")) {
					int dir = (int) targetObj["dir"];
					((PlaceableBig) target).SetDir((PlaceableBig.Direction) dir);
				}
			}
			
			listObj.Add(target);
		}
		// GD.Print("finished reading objects from file");
		// update layer's objectList
		Layer.objectList = listObj;
		
		// now: handle the uneditable tiles list
 		// Layer.editableTiles = new bool[levelDimensions.X, levelDimensions.Y];
		// array of Vector2I, representing each protected tile
		
		// GD.Print("now reading protected tiles from file");
		Layer.protectedTiles = new Vector2I[uneditableTiles.Count];
		
		for (int i = 0; i < uneditableTiles.Count; i++) {
			Godot.Collections.Array coords = (Godot.Collections.Array) uneditableTiles[i];
			Layer.protectedTiles[i] = new Vector2I((int) coords[0], (int) coords[1]);
		}
		// GD.Print("finished reading protectedTiles from file");
		
		// no need to directly update a layer's protectedTiles array, already done above
	}
	
	// used by some other process to get the object list of a specified layer
	// used for constructing a level after a save is loaded!
	// accepts params: "claw", "factory", "floor", "movement", "rail"
	public ArrayList GetObjectList(string layer) {
		// lazily take a layer and determine the correct response accordingly
		
		// "else" statements not necessarily needed here
		if (layer == "claw") {
			return liClaw.objectList;
		}
		if (layer == "factory") {
			return liFactory.objectList;
		}
		if (layer == "floor") {
			return liFloor.objectList;
		}
		if (layer == "movement") {
			return liMovement.objectList;
		}
		if (layer == "rail") {
			return liRail.objectList;
		}
		
		return null;
	}
	
	// similar to above, but gets the protected tiles of a given layer
	// accepts params: "claw", "factory", "floor", "movement", "rail"
	public Vector2I[] GetProtectedTiles(string layer) {
		// lazily take a layer and determine the correct response accordingly
		
		// "else" statements not necessarily needed here
		if (layer == "claw") {
			return liClaw.protectedTiles;
		}
		if (layer == "factory") {
			return liFactory.protectedTiles;
		}
		if (layer == "floor") {
			return liFloor.protectedTiles;
		}
		if (layer == "movement") {
			return liMovement.protectedTiles;
		}
		if (layer == "rail") {
			return liRail.protectedTiles;
		}
		
		return null;
	}
	
	// generate a single line Godot Variant dictionary that's easily translatable by Json
	public Godot.Collections.Dictionary<string, Variant> GenerateLine(string name, Variant obj) {
		Godot.Collections.Dictionary<string, Variant> output = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ name, obj }
			};
		
		return output;
	}
	
	// save a single line to a the provided file
	public void SaveLine(FileAccess saveFile, string name, Variant obj) {
		GD.Print("Saving ", name);
		saveFile.StoreLine(Json.Stringify(GenerateLine(name, obj)));
	}
	
	// generate essential data structure for a given layer
	private LayerInfo GenerateLayerInfo(Layer input, Vector2I dimensions) {
		return new LayerInfo(
			input.exportTiles(),
			input.exportEditableTiles(),
			input.exportObjectList(),
			dimensions //input.exportDimensions()
		);
	}
	
	private LayerInfo GenerateLayerInfo(Layer input) {
		return GenerateLayerInfo(input, input.exportDimensions());
	}
	
	// class that contains only the bare minimum required information to store layer info and etc
	// (basically just a data structure)
	private class LayerInfo {
		public PlaceableObject[,] tiles { get; set; }
		public bool[,] editableTiles { get; set; }
		public ArrayList objectList { get; set; }
		
		// ONLY USEFUL FOR LOADING
		// i.e. more efficient to save only the protected tiles (tiles are "unprotected" by default)
		public Vector2I[] protectedTiles { get; set; }
		
		// note: although all layers should have the same dimensions, still keep track/layer
		// also important to let system iterate through all tiles anyways
		int maxX;
		int maxY;
		
		public LayerInfo() {
			// default constructor; do nothing
		}
		
		public LayerInfo(PlaceableObject[,] tiles, bool[,] editableTiles, ArrayList objectList, Vector2I dimensions) {
			this.tiles = tiles;
			this.editableTiles = editableTiles;
			this.objectList = objectList;
			this.maxX = dimensions.X;
			this.maxY = dimensions.Y;
		}
		
		// PROTOTYPE:
		// generates a serialized object list
		// calls 'Save()' on every PlaceableObject on the object list and combines them all into
		// a single Godot array; can then be handled by Json.stringify().
		public Godot.Collections.Array serializeObjectList() {
			// Godot.Collections.Dictionary<string, Variant> res2 = new Godot.Collections.Dictionary<string, Variant>();
			Godot.Collections.Array res = new Godot.Collections.Array();	// reminder: this can hold ANY type of Godot "Variant" types
			// inefficient, but if we only have to use this per save/load cycle, not a huge problem
			
			// slight inefficiency by not checking if object map is valid here
			// regardless, function will return if map has already been initialized.
			// BoilerTronicsData.initializeObjectMap();
			
			int listCount = objectList.Count;
			for (int i = 0; i < listCount; i++) {
				// every item in the ArrayList will be a 'Placeable' object
				PlaceableObject extract = (PlaceableObject) objectList[i];
				
				// serialize the extracted PlaceableObject
				res.Add(extract.Save());
			}
			
			return res;
		}
		
		// PROTOTYPE:
		// generates a serialized list of uneditable tiles, coordinates stored in a Godot array
		// returns uneditable tiles because assumption is that all tiles are editable by default!
		// can then be processed by Json.stringify().
		public Godot.Collections.Array serializeUneditableList() {
			Godot.Collections.Array res = new Godot.Collections.Array();
			
			// inefficient O(maxX * maxY)
			// goes through all coordinates and adds any to the list if the grid is not editable at that location
			for (int x = 0; x < maxX; x++) {
				for (int y = 0; y < maxY; y++) {
					if (!editableTiles[x, y]) {
						res.Add(new int[]{x, y});
					}
				}
			}
			
			return res;
		}
		
		// PROTOTYPE:
		// serializes all of a layer's data into a single Godot Variant dictionary.
		public Godot.Collections.Dictionary<string, Variant> SerializeData()
		{
			return new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "objects", serializeObjectList() },
				{ "uneditableTiles", serializeUneditableList() },
			};
		}
	}
}
