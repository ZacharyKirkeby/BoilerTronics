using Godot;
using System;
using BoilerTronicsObjects.Placeable;
using BoilerTronicsObjects.Objects.ClawLayerObjects;
using BoilerTronicsObjects.Layers;

public partial class MovingObject : Area2D {

	Vector2 PosDelta; // amount we need to move (relative to global position)
	double TotalDelta; // Total time that has elapsed
	double TargetDelta; // Time that we want the movement to take
	Vector2I TargetPos; // This is the target grid position
	Vector2 CurrGlobalPos;
	Vector2 TargetGlobalPos;
	CollisionShape2D Shape;
	Sprite2D Sprite;
	public PlaceableObject obj; // Object that is moving
	public Layer layer; // Layer that object belongs to
	bool collided = false;

	bool move = true;

	public MovingObject(PlaceableObject obj, Vector2I mov, Layer layer, float time) {
		GD.Print("new moving!");

		float xd = mov.X;
		float yd = mov.Y;
		// We may also need to set the position of the object globally
		this.obj = obj;
		this.layer = layer;

		Vector2I pos = obj.GetCurrPos();
		GD.Print("POS: ", pos);

		this.TargetPos = pos + mov;
		GD.Print("TARGET POS: ", TargetPos);
		this.TargetDelta = time;

		// Why trhe fuck does this work??? IDK why we need to divide then multiply by 2, but we need to :D

		Vector2 currLocalPos = layer.MapToLocal(pos) / 2;

		this.CurrGlobalPos = currLocalPos;
		GD.Print("GLOBAL POS: ", CurrGlobalPos);
		GD.Print("LAYER POS: ", layer.ToGlobal(layer.Position));

		Vector2 targetLocalPos = layer.MapToLocal(TargetPos) / 2;

		this.TargetGlobalPos = targetLocalPos;
		GD.Print("TARGET GLOBAL POS: ", TargetGlobalPos);

		this.PosDelta = (TargetGlobalPos - CurrGlobalPos); // Calculate the amount we need to move
		GD.Print("POS DELTA: ", PosDelta);

		this.Position = CurrGlobalPos + layer.Position / 2;
	}

	public override void _Ready() {
		GD.Print("Ready!");
		this.layer.RemoveObject(this.obj);
		// Create collision object 2d
		this.Shape = new CollisionShape2D();

		// Create collision circle 2d
		Shape.Shape = new CircleShape2D();
		Shape.Position = this.Position;
		CircleShape2D circle = Shape.Shape as CircleShape2D;
		circle.Radius = 2; // 32 pixels (height of the objects)

		this.AddChild(Shape);
		Shape.ZIndex = layer.ZIndex;

		// Create sprite
		Sprite = new Sprite2D();
		if (this.obj is PlaceableFramed fObj) {
			Sprite.Texture = fObj.GetTexture() as Texture2D;
		} else {
			Sprite.Texture = this.obj.GetTexture() as Texture2D;
		}
		Sprite.Offset = new Vector2(0, 24);
		Sprite.Position = this.Position;
		// Sprite.Scale = new Vector2(10, 10);

		this.AddChild(Sprite);
		Sprite.ZIndex = layer.ZIndex;

		GD.Print("Children: ", this.GetChildren());
		GD.Print("POS: ", this.Position);

		// Register with the GameState (For resets and errors and such)
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
		manager.currLevel.RegisterMoving(this);
		manager.currLevel.runSem.Wait();

		// add event for when we detect a collision
		AreaEntered += Collison;
	}

	public override void _Process(double delta)
	{
		if (!move) {
			base._Process(delta);
			return;
		}

		this.TotalDelta += delta;
		BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;

		if (this.TotalDelta >= this.TargetDelta) {
			// We done
			// Move the object internally
			obj.MoveCurrPos(TargetPos.X, TargetPos.Y);

			//check for static collision
			if (layer != null)
			{
				var existingObj = layer.FindObject(TargetPos);
				if (existingObj != null && existingObj != obj)
				{
					manager.currLevel.MovingCollisionReport(this);
					return;
				}
			}

			if (obj is ClawObject cObj) {
				cObj.moving = false;
			}

			// Place the object back on the layer
			layer.AddObject(obj);
			// De-register object from the game state
			//BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.UnRegisterMoving(this);
			manager.currLevel.runSem.Release();
			// Destroy this object
			this.QueueFree();
		}

		// We need to move
		Vector2 CurrPos = this.Position;
		CurrPos += this.PosDelta * ((float) (delta/TargetDelta)); // Calculate new pos for this step
		this.Position = CurrPos; // set new pos
		Shape.Position = CurrPos;
		Sprite.Position = CurrPos;

		base._Process(delta);
	}

	public void Collison(Node2D body) {
		GD.Print("Ouch!");
		if (body is MovingObject mBody && mBody.layer != this.layer) return; // we only wnat to colide thing on the same layer

		if (collided) return; // Don't do anything if this is already handled
		collided = true; // Set flag for this collision
		// We have collided with something else, this is a problem and shouldn't happen :(
		// This will trigger an error and then halt all movement
		if (body is MovingObject otherMoving) {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.MovingCollisionReport(this, otherMoving);
		}
		else {
			BoilerTronicsGlobalManager manager = BoilerTronicsGlobalManager.GlobalManager;
			manager.currLevel.MovingCollisionReport(this);
		}
	}

	public void Halt() {
		// make this flag false so that stop moving
		move = false;
	}
}
