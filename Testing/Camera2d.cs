using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		private bool rmbHeld = false;
		private Vector2 maxZoom = new Vector2((float) 3.0, (float) 3.0);
		private Vector2 minZoom = new Vector2((float) 0.5, (float) 0.5);
		
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
						
						Vector2 zoomIn = this.GetZoom() + new Vector2((float) 0.1, (float) 0.1);
						if (zoomIn < maxZoom) {
							this.SetZoom(zoomIn); 
						}
					} else if (buttonEvent.ButtonIndex == MouseButton.WheelDown) {
						// zoom out
						
						Vector2 zoomOut = this.GetZoom() - new Vector2((float) 0.1, (float) 0.1);
						if (zoomOut > minZoom) {
							this.SetZoom(zoomOut); 
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
