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
			// objectMap.Add(hashCoords(0, new Vector2I(0, 3)), 3);	//factorymachine
			objectMap.Add(hashCoords(3, new Vector2I(0, 0)), 4);	//factoryfurnace
			objectMap.Add(hashCoords(3, new Vector2I(0, 3)), 5);	//factorypress
			objectMap.Add(hashCoords(3, new Vector2I(0, 2)), 6);	//factoryroller
			objectMap.Add(hashCoords(13, new Vector2I(0, 0)), 7);	//pipe
			objectMap.Add(hashCoords(14, new Vector2I(0, 0)), 8);	//waterpump
			objectMap.Add(hashCoords(14, new Vector2I(0, 1)), 9);	//lubepump
			
			// Actual Floors
			objectMap.Add(hashCoords(4, new Vector2I(0, 0)), 2);	//floordefault
			
			// Floor Obstructions
			objectMap.Add(hashCoords(5, new Vector2I(0, 0)), 30);	//FloorCrackedTileObject
			objectMap.Add(hashCoords(5, new Vector2I(0, 3)), 31);	//PipeBrokenFloorObject - Left
			objectMap.Add(hashCoords(5, new Vector2I(1, 3)), 32);	//PipeBrokenFloorObject - Right
			
			// Materials
			// objectMap.Add(hashCoords(0, new Vector2I(0, 4)), 200);	//factorymaterial
			
			
			// Claw/rail layer
			objectMap.Add(hashCoords(1, new Vector2I(0, 0)), 50);	//clawdefault
			objectMap.Add(hashCoords(1, new Vector2I(0, 1)), 100);	//railleftdefault
			objectMap.Add(hashCoords(1, new Vector2I(0, 2)), 101);	//railrightdefault
			
			// Ceiling (Rail Layer) Obstructions
			objectMap.Add(hashCoords(5, new Vector2I(0, 1)), 130);	//StalagmiteObject
			objectMap.Add(hashCoords(5, new Vector2I(1, 1)), 131);	//StalagmiteObjects
			objectMap.Add(hashCoords(5, new Vector2I(0, 2)), 132);	//PipeBrokenCeilingObject - Left
			objectMap.Add(hashCoords(5, new Vector2I(1, 2)), 133);	//PipeBrokenCeilingObject - Right
			
			
			// Movement layer
			objectMap.Add(hashCoords(2, new Vector2I(0, 0)), 150); //conveyorleftdefault
			objectMap.Add(hashCoords(2, new Vector2I(0, 1)), 151); //conveyorrightdefault
			objectMap.Add(hashCoords(2, new Vector2I(0, 2)), 152); //rotatordefault
			objectMap.Add(hashCoords(2, new Vector2I(2, 0)), 153); //switch

			// Materials
			objectMap.Add(hashCoords(9, new Vector2I(0, 0)), 200); //coal
			objectMap.Add(hashCoords(9, new Vector2I(1, 0)), 201); //ironore
			objectMap.Add(hashCoords(9, new Vector2I(2, 0)), 202); //ironbar
			objectMap.Add(hashCoords(9, new Vector2I(3, 0)), 203); //ironplate
			objectMap.Add(hashCoords(9, new Vector2I(0, 1)), 204); //ironrod
			
			// Input Objects
			objectMap.Add(hashCoords(0, new Vector2I(1, 0)), 250); 	//factoryin - coal
			objectMap.Add(hashCoords(0, new Vector2I(2, 0)), 251); 	//factoryin - iron ore
			objectMap.Add(hashCoords(0, new Vector2I(3, 0)), 252); 	//factoryin - iron bar
			objectMap.Add(hashCoords(0, new Vector2I(0, 2)), 253); 	//factoryin - iron plate
			objectMap.Add(hashCoords(0, new Vector2I(1, 2)), 254); 	//factoryin - iron rod
			
			// Output Objects
			objectMap.Add(hashCoords(0, new Vector2I(1, 1)), 300); 	//factoryout - coal
			objectMap.Add(hashCoords(0, new Vector2I(2, 1)), 301); 	//factoryout - iron ore
			objectMap.Add(hashCoords(0, new Vector2I(3, 1)), 302); 	//factoryout - iron bar
			objectMap.Add(hashCoords(0, new Vector2I(0, 3)), 303); 	//factoryout - iron plate
			objectMap.Add(hashCoords(0, new Vector2I(1, 3)), 304); 	//factoryout - iron rod
			
			hasInitializedObjectMap = true;
		}
		
		// object ID reservations: (for object factory):
		// 0-49: 	floor layer
		// 50-99: 	claw layer
		// 100-149:	rail layer
		// 150-199:	movement layer
		// 200-249: items
		// 250-299: input items (ignoring basic input obj)
		// 300-349: output items (ignoring basic output obj)
		
		// not exactly a perfect system, but so long as no single value exceeds ~1000,
		// this will return a unique value very time.
		public static int hashCoords(int sourceId, Vector2I atlasPos) {
			int res = sourceId;
			res += atlasPos.X * 1000;
			res += atlasPos.Y * 1000000;
			
			// GD.Print("hash coords: input: ", sourceId, ", atlasPos: ", atlasPos);
			// GD.Print("hash coords: output: ", res, "\n");
			return res;
		}
	}
}
