using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Layers;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Data;

// TODO: design distinct file system for auto saves, per level saves, etc

public class BoilerTronicsSaveState
{
	// This should define what the save state of the current level is
	// We should serialize this and de-serialize it to save that level
	int save_slot = -1; // -1 = base level; 0-2 = the respecive save slot for the user; any other value should result in an error (TODO: implement such errors)
	int level_id = 0; // id for which level this save is referring tod; // id for which level this save is referring to
	
	private Vector2I levelDimensions;
	private LayerInfo liClaw;
	private LayerInfo liFactory;
	private LayerInfo liFloor;
	private LayerInfo liMovement;
	private LayerInfo liRail;
	
	// LAZY SAVE IMPLEMENTATION: save all the data completely raw.
	
	public BoilerTronicsSaveState() {
		// do nothing (for now)
	}
	
	public void SetLevelDimensions(Vector2I max) {
		levelDimensions = max;
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
	
	public void SaveData(BoilerTronicsGlobalManager manager) {
		// lazy; import public data straight from manager
		
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
		using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
		
		Godot.Collections.Dictionary<string, Variant> metadata = 
			new Godot.Collections.Dictionary<string, Variant>()
			{
				{ "mapSize", new int[]{levelDimensions.X, levelDimensions.Y} },
				{ "levelId", level_id },
				{ "saveSlot", save_slot },
			};
		
		// now: save important information line-by-line!
		SaveLine(saveFile, "metadata", metadata);
		SaveLine(saveFile, "clawLayer", liClaw.SerializeData());
		SaveLine(saveFile, "factoryLayer", liFactory.SerializeData());
		SaveLine(saveFile, "floorLayer", liFloor.SerializeData());
		SaveLine(saveFile, "movementlayer", liMovement.SerializeData());
		SaveLine(saveFile, "railLayer", liRail.SerializeData());
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
		PlaceableObject[,] tiles { get; }
		bool[,] editableTiles { get; }
		ArrayList objectList { get; }
		
		// note: although all layers should have the same dimensions, still keep track/layer
		// also important to let system iterate through all tiles anyways
		int maxX;
		int maxY;
		
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
			BoilerTronicsData.initializeObjectMap();
			
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
