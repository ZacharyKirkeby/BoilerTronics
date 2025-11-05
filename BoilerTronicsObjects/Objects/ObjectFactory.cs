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
				return null;
			}
			
			
			return GenerateObject(objectId, x, y);
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
				case 3:
					//factory machine
					return new FactoryTestMachine(x, y, 0);
				case 4:
					//floordefault
					return new FactoryTestMaterial(x, y, 0);
				case 50:
					//clawdefault
					return new ClawObject(x, y, 0);
				case 100:
					//railleftdefault
					return new TrackObject(x, y, 0, 0);
				case 101:
					//railrightdefault
					return new TrackObject(x, y, 1, 0);
				case 150:
					//conveyorleftdefault
					return new ConveyorObject(x, y, 0, 0);
				case 151:
					//conveyorrightdefault
					return new ConveyorObject(x, y, 1, 0);
				case 152:
					//rotatordefault
					return new ConveyorRotatorObject(x, y, 0);
				default:
					GD.Print("ERROR: catastrophic failure from ObjectFactory");
					return null;
			}
		}
	}

}
