using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// tracks right mouse button held
		private bool rmbHeld = false;
		
		private const float zoomFactor = 0.1f;
		 
		// additive zoom
		private Vector2 zoomFloat = new Vector2(zoomFactor,  zoomFactor);
		
		// multiplicative zoom
		private Vector2 zoomMultP = new Vector2(1f + zoomFactor, 1f + zoomFactor);
		private Vector2 zoomMultN = new Vector2(1f - zoomFactor, 1f - zoomFactor);
		
		private Vector2 maxZoom = new Vector2((float) 3.0, (float) 3.0);
		private Vector2 minZoom = new Vector2((float) 0.5, (float) 0.5);
		
		private Vector2 errorPosition = new Vector2(150,500);
		private Sprite2D errorSprite;


		// general zoom function to zoom specifically at the mouse
		public void zoomToTarget(Vector2 zoomTarget) {
			// move zoom target to center of screen
			Vector2 p0 = GetLocalMousePosition();
			this.Zoom = zoomTarget;
			Vector2 p1 = GetLocalMousePosition();
			Vector2 diff = p1 - p0;
			// GD.Print("diff: ",  diff);
			this.Position -= diff;
		}
		
		// handle mouse inputs (RMB, scroll wheel)
		public override void _Input(InputEvent @event) {
			// Note: these inputs will still "pass through" and Layer.cs could theoretically receive this (?)
			// Be advised!

			// all mouse button events
			if (@event is InputEventMouseButton buttonEvent) {
				
				// catch all button events related to RMB
				if (buttonEvent.ButtonIndex == MouseButton.Right) {
					if (buttonEvent.Pressed) {
						rmbHeld = true;
					} else {
						rmbHeld = false;
					}
				}
				
				// "@event.IsPressed()" is important to avoid double-catching inputs!
				// only works if we're looking to see if the event was pressed, however
				if (@event.IsPressed()) {
					if (buttonEvent.ButtonIndex == MouseButton.WheelUp) {
						// zoom in
						Vector2 zoomIn = this.GetZoom() * zoomMultP;//this.GetZoom() + zoomFloat;
						if (zoomIn < maxZoom) {
							// GD.Print("zoom in");
							zoomToTarget(zoomIn);
						}
					} else if (buttonEvent.ButtonIndex == MouseButton.WheelDown) {
						// zoom out
						Vector2 zoomOut = this.GetZoom() * zoomMultN;//this.GetZoom() - zoomFloat;
						if (zoomOut > minZoom) {
							// GD.Print("zoom out");
							zoomToTarget(zoomOut);
						}
					}
				}
				// end
			}
			
			
			if (@event is InputEventMouseMotion eventMouseMotion && rmbHeld) {
				// need to account for zoom level!
				this.Position += eventMouseMotion.GetScreenRelative() * -1 / this.GetZoom().X;//new Vector2(5, 5);
			}
		}
			
		public void SpawnErrorSprite(Vector2 errorPosition) {
			//if coords are negative/invalid do not spawn
			if(errorPosition.X < 0 || errorPosition.Y < 0) {
				return;
			}

			//remove sprite if already there
			if (errorSprite != null && IsInstanceValid(errorSprite)) {
				errorSprite.QueueFree();
				errorSprite = null;
			}

			errorSprite = new Sprite2D();
			errorSprite.Texture = GD.Load<Texture2D>("res://Resources/Icons/exclamation.png");
			errorSprite.Position = errorPosition;
			
			errorSprite.ZIndex = 1000;      
			errorSprite.ZAsRelative = false;
			
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
