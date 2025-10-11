using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		private bool rmbHeld = false;
		private Vector2 maxZoom = new Vector2((float) 3.0, (float) 3.0);
		private Vector2 minZoom = new Vector2((float) 0.5, (float) 0.5);
		
		private Vector2 errorPosition = new Vector2(150,500);
		private Sprite2D errorSprite;
		
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
			
		public void SpawnErrorSprite(Vector2 errorPosition) {
			//remove sprite if already there
			if (errorSprite != null && IsInstanceValid(errorSprite)) {
				errorSprite.QueueFree();
				errorSprite = null;
			}

			errorSprite = new Sprite2D();
			errorSprite.Texture = GD.Load<Texture2D>("res://Resources/exclamation.png");
			errorSprite.Position = errorPosition;
			
			//scale down
			errorSprite.Scale = new Vector2(0.15f, 0.15f);

			//have error notice display on level ui
			this.GetParent().AddChild(errorSprite);
		}
		
		public void RemoveErrorSprite() {
			if (errorSprite != null && IsInstanceValid(errorSprite)) {
				errorSprite.QueueFree();
				errorSprite = null;
			}
		}
	}
}
