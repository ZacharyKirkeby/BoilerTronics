using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Data;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects
{
	public class ObjectFactory
	{		
		
		
		// note: as of the current implementation, this isn't really a good factory in the strictest sense
		// TODO: implement version that accepts alt titles
		public static PlaceableObject CreateObject(Vector2I originPos, int sourceId, Vector2I atlasPos) {			
			// TODO: creator/main factory function
			// given which "sourceId" (i.e. which atlas map to pull from) -- this will determine the object's layer
			// and given the "atlasPos" (i.e. where on the atlas the object is) -- this will determine the identify of the object (i.e. how Terraria does it)
			
			if (!BoilerTronicsData.hasInitializedObjectMap) {
				BoilerTronicsData.initializeObjectMap();
			}
			
			int x = originPos[0];
			int y = originPos[1];
			// edit: originPos.X/originPos.Y should also work
			
			// (pretend there's some fancy switch/if statements that take in source iDs and atlasPos
			// to identify an actual object
			
			PlaceableObject target = null;
			int hashedCoords = BoilerTronicsData.hashCoords(sourceId, atlasPos);
			
			// TODO: catch exception from nonexistant coords
			int objectId;
			bool gotID = BoilerTronicsData.objectMap.TryGetValue(hashedCoords, out objectId);
			
			if (!gotID) {
				GD.Print("ERROR: catastrophic failure from ObjectFactory, could not find target object");
				GD.Print("Failed to find: " + atlasPos + ", sourceID: ", sourceId);
				return null;
			}
			
			
			return GenerateObject(objectId, x, y);
		}

		public static List<PlaceableBigData> GetBigObjectTileMap(int objectID, PlaceableBig.Direction dir) {
			switch (objectID) {
				case 4:
					//factory furnace
					return FactoryFurnace.StaticGetTextureGrid(dir);
				case 5:
					//factory press
					return FactoryPress.StaticGetTextureGrid(dir);
				case 6:
					//factory roller
					return FactoryRoller.StaticGetTextureGrid(dir);
				case 153:
					//switch
					return SwitchObject.StaticGetTextureGrid(dir);
			}

			return null;
		}

		public static PlaceableObject GenerateObject(int objectID, int x = 0, int y = 0) {
			switch (objectID) {
				case -1:
					return null;
				case 0:
					//factoryin
					return new FactoryInputObject(x, y, 0);
				case 1:
					//factoryout
					return new FactoryOutputObject(x, y, 0);
				case 2:
					//floordefault
					return new FloorTileObject(x, y, 0);
				// case 3:
					//factory machine - DISABLED
					// return new FactoryTestMachine(x, y, 0);
				case 4:
					//factory furnace
					return new FactoryFurnace(x, y, 0);
				case 5:
					//factory furnace
					return new FactoryPress(x, y, 0);
				case 6:
					//factory furnace
					return new FactoryRoller(x, y, 0);
				case 30:
					//FloorCrackedTileObject
					return new FloorCrackedTileObject(x, y, 0);
				case 31:
					//PipeBrokenFloorObject - Left
					return new PipeBrokenFloorObject(x, y, 0, 0);
				case 32:
					//PipeBrokenFloorObject - Right
					return new PipeBrokenFloorObject(x, y, 1, 0);
				case 50:
					//clawdefault
					return new ClawObject(x, y, 0);
				case 100:
					//railleftdefault
					return new TrackObject(x, y, 0, 0);
				case 101:
					//railrightdefault
					return new TrackObject(x, y, 1, 0);
				case 130:
					//StalagmiteObject
					return new StalagmiteObject(x, y, 0);
				case 131:
					//StalagmitesObject
					return new StalagmitesObject(x, y, 0);
				case 132:
					//PipeBrokenCeilingObject - Left
					return new PipeBrokenCeilingObject(x, y, 0, 0);
				case 133:
					//PipeBrokenCeilingObject - Right
					return new PipeBrokenCeilingObject(x, y, 1, 0);
				case 150:
					//conveyorleftdefault
					return new ConveyorObject(x, y, 0, 0);
				case 151:
					//conveyorrightdefault
					return new ConveyorObject(x, y, 1, 0);
				case 152:
					//rotatordefault
					return new ConveyorRotatorObject(x, y, 0);
				case 153:
					//rotatordefault
					return new SwitchObject(x, y, 0);
				case 200:
					//Coal
					return new CoalObject(x, y, 0);
				case 201:
					//IronOre
					return new IronOreObject(x, y, 0);
				case 202:
					//IronBar
					return new IronBarObject(x, y, 0);
				case 203:
					//IronPlate
					return new IronPlateObject(x, y, 0);
				case 204:
					//IronRod
					return new IronRodObject(x, y, 0);
				case 250:
					//factoryin - coal
					return new FactoryInputObject(x, y, 0, 200);
				case 251:
					//factoryin - iron ore
					return new FactoryInputObject(x, y, 0, 201);
				case 252:
					//factoryin - iron bar
					return new FactoryInputObject(x, y, 0, 202);
				case 253:
					//factoryin - iron plate
					return new FactoryInputObject(x, y, 0, 203);
				case 254:
					//factoryin - iron rod
					return new FactoryInputObject(x, y, 0, 204);
				case 300:
					//factoryout - coal
					return new FactoryOutputObject(x, y, 0, 200);
				case 301:
					//factoryout - iron ore
					return new FactoryOutputObject(x, y, 0, 201);
				case 302:
					//factoryout - iron bar
					return new FactoryOutputObject(x, y, 0, 202);
				case 303:
					//factoryout - iron plate
					return new FactoryOutputObject(x, y, 0, 203);
				case 304:
					//factoryout - iron rod
					return new FactoryOutputObject(x, y, 0, 204);
				default:
					GD.Print("ERROR: catastrophic failure from ObjectFactory");
					return null;
			}
		}
	}

}
