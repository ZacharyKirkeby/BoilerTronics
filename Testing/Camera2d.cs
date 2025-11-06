using Godot;
using System;

namespace BoilerTronicsObjects.GameCamera {
	public partial class Camera2d : Camera2D
	{
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		
		// tracks right mouse button held
		private bool rmbHeld = false;
		
		// camera offset to reach the center of the screen
		// viewport size is (1408, 750)
		private const float cameraOffsetY = - 750 / 2;
		private static Vector2 cameraOffset = new Vector2(0, cameraOffsetY);
		
		// how much beyond the min/max of a level the camera can move
		private const float cameraBoundaryBufferAmount = 0;
		private static Vector2 cameraBoundaryBuffer = new Vector2(cameraBoundaryBufferAmount, cameraBoundaryBufferAmount);
		
		// general multiplier for the boundary of the camera
		private const float cameraBoundaryMult = 0.9f;
		
		// zoom factor
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
		
		
		
		public override void _Ready() {
			// on startup, move the camera to the "center" of the view area
			this.Position -= new Vector2I(0, 750 / 2);
			GD.Print("Camera2d: pos: ", this.Position);
			validateScreenPos();
		}


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
		
		
		// makes sure that the screen is within "bounds"
		public void validateScreenPos() {
			// try and get the global manager
			// if it still doesn't exist, crash.
			if (manager == null) {
				manager = BoilerTronicsGlobalManager.GlobalManager;
				
				if (manager == null) { return; }
			}
			
			if (manager.currLevel == null) {
				GD.Print("Camera2d: manager.currLevel == null, aborting validateScreePos()");
				return;
			}
		
			// cameraOffset = GetViewport().GetVisibleRect().Size;
			// GD.Print("camera size: ", GetViewport().GetVisibleRect().Size);
			// GD.Print("camera size2: ", GetParent().GetViewport().GetVisibleRect().Size);
			
			// get min, max coordinates
			// these are the local positions of the floor tilemap's edges
			Vector2 minCoords = manager.currLevel.minCoords;
			Vector2 maxCoords = manager.currLevel.maxCoords;
			
			
			
			// apply camera offset, camera boundary buffer to calculate the min/max valid coordinates that
			// the camera can exist in
			// fancy math to handle camera "boundaries" correctly, because in reality the camera is glued to the middle-top of the play area
			Vector2 minValidCoords = ((minCoords - cameraBoundaryBuffer) * this.GetZoom().X * cameraBoundaryMult + cameraOffset / this.GetZoom().X);
			Vector2 maxValidCoords = ((maxCoords + cameraBoundaryBuffer) * this.GetZoom().X * cameraBoundaryMult + cameraOffset / this.GetZoom().X);
			
			// GD.Print("\ncurr camera coords: ", this.Position);
			// GD.Print("min camera coords: ", minValidCoords);
			// GD.Print("max camera coords: ", maxValidCoords);
			// GD.Print("min base coords: ", minCoords);
			// GD.Print("max base coords: ", maxCoords);
			
			// clamp position to min/max
			float newPosX = Mathf.Clamp(this.Position.X, minValidCoords.X, maxValidCoords.X);
			float newPosY = Mathf.Clamp(this.Position.Y, minValidCoords.Y, maxValidCoords.Y);
			this.Position = new Vector2(newPosX, newPosY);
			
			// restrict camera panning
			/*
			if (this.Position.X < minValidCoords.X) { 
				this.SetPosition(new Vector2(minValidCoords.X, this.Position.Y));
			}
			if (this.Position.Y < minValidCoords.Y) { 
				this.SetPosition(new Vector2(this.Position.X, minValidCoords.Y));
			}
			if (this.Position.X > maxValidCoords.X) { 
				this.SetPosition(new Vector2(maxValidCoords.X, this.Position.Y));
			}
			if (this.Position.Y > maxValidCoords.Y) { 
				this.SetPosition(new Vector2(this.Position.X, maxValidCoords.Y));
			}
			*/
			// GD.Print("updated camera coords: ", this.Position, "\n");
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
					
					validateScreenPos();
				}
				// end
			}
			
			
			if (@event is InputEventMouseMotion eventMouseMotion && rmbHeld) {
				// need to account for zoom level!
				this.Position += eventMouseMotion.GetScreenRelative() * -1 / this.GetZoom().X;//new Vector2(5, 5);
			
				validateScreenPos();
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
