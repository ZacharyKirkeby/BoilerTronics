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
			
			switch (objectId) {
				case -1:
					target = null;
					GD.Print("ERROR: input objectId was invalid!");
					GD.Print("sequence: %d, (%d, %d)", sourceId, atlasPos.X, atlasPos.Y);
					break;
				case 0:
					//factoryin
					target = new FactoryInputObject(x, y, 0);
					break;
				case 1:
					//factoryout
					target = new FactoryOutputObject(x, y, 0);
					break;
				case 2:
					//floordefault
					target = new FloorTileObject(x, y, 0);
					break;
				case 50:
					//clawdefault
					target = new ClawObject(x, y, 0);
					break;
				case 100:
					//railleftdefault
					target = new TrackObject(x, y, 0, 0);
					break;
				case 101:
					//railrightdefault
					target = new TrackObject(x, y, 1, 0);
					break;
				case 150:
					//conveyorleftdefault
					target = new ConveyorObject(x, y, 0, 0);
					break;
				case 151:
					//conveyorrightdefault
					target = new ConveyorObject(x, y, 1, 0);
					break;
				case 152:
					//rotatordefault
					target = new ConveyorRotatorObject(x, y, 0);
					break;
				case '_':
					GD.Print("ERROR: catastrophic failure from ObjectFactory");
					break;
			}
			
			return target;//new ClawObject(x, y, 0);
		}
	}
}
