using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		private bool rmbHeld = false;
		private float zoomDiff = (float) 0.05;
		private Vector2 zoomFloat = new Vector2((float) 0.05, (float) 0.05);
		
		private Vector2 maxZoom = new Vector2((float) 3.0, (float) 3.0);
		private Vector2 minZoom = new Vector2((float) 0.5, (float) 0.5);
		
		// general zoom function to zoom specifically at the mouse; TODO
		public void zoomToTarget(Vector2 zoomTarget) {
			
			// note: following code is from the Godot forum -- will need tweaking!
			/*
			Vector2 halfSize = GetViewport().GetVisibleRect().Size / (float) 2.0;
			Vector2 cameraPosition = this.Position;
			Vector2 point = GetLocalMousePosition();
			Vector2 newCameraPosition;
			Vector2 z0 = this.Zoom;
			Vector2 z1 = zoomTarget;

			newCameraPosition = cameraPosition + (-halfSize + point) * (z0 - z1);
			this.SetZoom(zoomTarget);
			this.Position = newCameraPosition;
			*/
			
			// Vector2 p0 = GetLocalMousePosition();
			this.Zoom = zoomTarget;
			// Vector2 p1 = GetLocalMousePosition();
			// Vector2 diff = p0 - p1;
			// GD.Print("diff: ",  diff);
			// this.Position += diff;

		}
		
		public override void _Input(InputEvent @event)
			{
				// Note: these inputs will still "pass through" and Layer.cs could theoretically receive this
				// Be advised!

				if (@event is InputEventMouseButton buttonEvent) {
					
					if (buttonEvent.ButtonIndex == MouseButton.Right) {
						if (buttonEvent.Pressed) {
							rmbHeld = true;
						} else {
							rmbHeld = false;
						}
					} else if (buttonEvent.ButtonIndex == MouseButton.WheelUp) {
						// zoom in
						
						Vector2 zoomIn = this.GetZoom() + zoomFloat;
						if (zoomIn < maxZoom) {
							zoomToTarget(zoomIn);
						}
					} else if (buttonEvent.ButtonIndex == MouseButton.WheelDown) {
						// zoom out
						
						Vector2 zoomOut = this.GetZoom() - zoomFloat;
						if (zoomOut > minZoom) {
							zoomToTarget(zoomOut);
						}
					}
				}
				
				if (@event is InputEventMouseMotion eventMouseMotion && rmbHeld) {
					// need to account for zoom level!
					this.Position += eventMouseMotion.GetScreenRelative() * -1 / this.GetZoom().X;//new Vector2(5, 5);
				}
			}
	}
}
