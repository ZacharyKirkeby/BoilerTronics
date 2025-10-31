using Godot;
using Godot.Collections;
using System;

// Holds important dictionaries for translating to/from tile IDs

namespace BoilerTronicsObjects.Data {
	public class BoilerTronicsData
	{
		public static Dictionary<int, int> objectMap = new Dictionary<int, int>();
		public static bool hasInitializedObjectMap = false;
		
		// set up the object map
		public static void initializeObjectMap() {
			
			if (hasInitializedObjectMap) {return;}
			
			// Floor layer
			objectMap.Add(hashCoords(0, new Vector2I(0, 0)), 0); 	//factoryin
			objectMap.Add(hashCoords(0, new Vector2I(0, 1)), 1);	//factoryout
			objectMap.Add(hashCoords(0, new Vector2I(0, 2)), 2);	//floordefault
			objectMap.Add(hashCoords(0, new Vector2I(0, 3)), 3);	//factorymachine
			objectMap.Add(hashCoords(0, new Vector2I(0, 4)), 4);	//factorymaterial
			
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
	}
}
