using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Objects.FactoryLayerObjects;
using BoilerTronicsObjects.Objects.MovementLayerObjects;
using BoilerTronicsObjects.Placeable;

namespace BoilerTronicsObjects.Objects
{
	public class ObjectFactory
	{
		public static Dictionary<int, int> objectMap = new Dictionary<int, int>();
		public static bool hasInitializedObjectMap = false;
		
		// set up the object map
		public static void initializeObjectMap() {
			// Floor layer
			objectMap.Add(hashCoords(0, new Vector2I(0, 0)), 0); 	//factoryin
			objectMap.Add(hashCoords(0, new Vector2I(0, 1)), 1);	//factoryout
			objectMap.Add(hashCoords(0, new Vector2I(0, 2)), 2);	//floordefault
			
			// Claw/rail layer
			objectMap.Add(hashCoords(1, new Vector2I(0, 0)), 50);	//clawdefault
			objectMap.Add(hashCoords(1, new Vector2I(0, 1)), 100);	//railleftdefault
			objectMap.Add(hashCoords(1, new Vector2I(0, 2)), 101);	//railrightdefault
			
			// Movement layer
			objectMap.Add(hashCoords(2, new Vector2I(0, 0)), 150); //conveyorleftdefault
			objectMap.Add(hashCoords(2, new Vector2I(0, 1)), 151); //conveyorrightdefault
			objectMap.Add(hashCoords(2, new Vector2I(0, 2)), 152); //rotatordefault
			
			hasInitializedObjectMap = true;
		}
		
		// object ID reservations: (for object factory):
		// 0-49: 	floor layer
		// 50-99: 	claw layer
		// 100-149:	rail layer
		// 150-199:	movement layer
		
		// not exactly a perfect system, but so long as no single value exceeds ~1000,
		// this will return a unique value very time.
		public static int hashCoords(int sourceId, Vector2I atlasPos) {
			int res = sourceId;
			res += atlasPos.X * 1000;
			res += atlasPos.Y * 1000000;
			
			return res;
		}
		
		// note: as of the current implementation, this isn't really a good factory in the strictest sense
		public static PlaceableObject CreateObject(Vector2I originPos, int sourceId, Vector2I atlasPos) {			
			// TODO: creator/main factory function
			// given which "sourceId" (i.e. which atlas map to pull from) -- this will determine the object's layer
			// and given the "atlasPos" (i.e. where on the atlas the object is) -- this will determine the identify of the object (i.e. how Terraria does it)
			
			if (!hasInitializedObjectMap) {
				initializeObjectMap();
			}
			
			int x = originPos[0];
			int y = originPos[1];
			// edit: originPos.X/originPos.Y should also work
			
			// (pretend there's some fancy switch/if statements that take in source iDs and atlasPos
			// to identify an actual object
			
			PlaceableObject target = null;
			int hashedCoords = hashCoords(sourceId, atlasPos);
			
			// TODO: catch exception from nonexistant coords
			int objectId;
			bool gotID = objectMap.TryGetValue(hashedCoords, out objectId);
			
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
					target = new TrackLeftObject(x, y, 0);
					break;
				case 101:
					//railrightdefault
					target = new TrackRightObject(x, y, 0);
					break;
				case 150:
					//conveyorleftdefault
					target = new ConveyorLeftObject(x, y, 0);
					break;
				case 151:
					//conveyorrightdefault
					target = new ConveyorRightObject(x, y, 0);
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
