using Godot;
using System;
using System.Collections;
using BoilerTronicsObjects.Objects;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects.MovementLayerObjects;

namespace BoilerTronicsObjects.Layers
{
	// public partial class MovementLayer(int x, int y) : Layer(x, y)
	public partial class MovementLayer : Layer
	{		
		public MovementLayer(int x, int y) : base(x,y) {
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public MovementLayer() : base() {
			// handle offsets for rendering protected tiles
			yRenderProtectedTileOffset = 10;
			protectedToggleMouseOffset = new Vector2(0f, -0f);
		}

		public override void _Input(InputEvent @event)
		{
			// i love it when layers don't stack on top of each other
			MouseInput(@event, 1);
			base._Input(@event);
		}
	}
}
